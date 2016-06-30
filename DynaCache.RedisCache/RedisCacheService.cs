using DynaCache.RedisCache.Internals;
using NLog;
using NLog.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace DynaCache.RedisCache
{
	public class RedisCacheService : IDynaCacheService, ICacheInvalidator
	{
		private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
		private readonly IRedisService _redisService;
		private readonly ICacheSerializer _serializer;
		private readonly IInvalidationDescriptor[] _invalidationDescriptors;
		private readonly IConcreteKeyPatternProvider _concreteKeyPatternProvider;

		public RedisCacheService(
			IRedisService redisService,
			ICacheSerializer serializer,
			IInvalidationDescriptor[] invalidationDescriptors,
			IConcreteKeyPatternProvider concreteKeyPatternProvider)
		{
			_redisService = redisService;
			_serializer = serializer;
			_invalidationDescriptors = invalidationDescriptors;
			_concreteKeyPatternProvider = concreteKeyPatternProvider;
		}

		public bool TryGetCachedObject<T>(string cacheKey, out T result)
		{
			using (new TracingLogProxy(logger))
			{
				result = default(T);
				logger.Debug($"cache for {cacheKey} requested");
				RedisValue res;
				try
				{
					res = _redisService.Database.StringGet(cacheKey);
				}
				catch (Exception e)
				{
					logger.Debug($"failed to retrieve cache for {cacheKey} due to {e}");
					return false;
				}
				var notFound = res.IsNull;
				if (notFound)
				{
					logger.Debug($"cache not found for {cacheKey}");
					return false;
				}
				var serialized = res.ToString();
				logger.Debug($"found cache for {cacheKey}");
				try
				{
					result = _serializer.Deserialize<T>(serialized);
					return true;
				}
				catch (Exception e)
				{
					logger.Error(e, $"failed to deserialize cache for {cacheKey}");
					return false;
				}
			}
		}

		public void SetCachedObject<T>(string cacheKey, T data, int duration)
		{
			using (new TracingLogProxy(logger))
			{
				var expiration = TimeSpan.FromSeconds(duration);
				logger.Debug($"storing cache for {cacheKey} with expiry {expiration}");
				var serialized = _serializer.Serialize(data);
				_redisService.Database.StringSet(cacheKey, serialized, expiration);
			}
		}

		public void InvalidateCache(object invalidObject)
		{
			var keyPatterns = _invalidationDescriptors
				.SelectMany(id => id.GetCommonKeyPatternsFrom(invalidObject))
				.Select(_concreteKeyPatternProvider.ConvertCommonKey)
				.ToList();
			var pageSize = _redisService.Configuration.GetPollingPageSize();
			var keys = _redisService.Servers
				.SelectMany(
					collectionSelector: _ => keyPatterns,
					resultSelector: (s, kp) => s.Keys(pattern: kp, pageSize: pageSize).ToList()) 
				.SelectMany(_ => _)
				.Distinct()
				.ToArray();
			var deleted = _redisService.Database.KeyDelete(keys);
			if (keys.Length > deleted)
				logger.Warn($"Redis found {keys.Length}, but deleted only {deleted}");
		}
	}
}