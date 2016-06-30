using DynaCache.RedisCache.Internals;
using NLog;
using NLog.Extension;
using StackExchange.Redis;
using System;
using System.Linq;

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
				catch (TimeoutException)
				{
					logger.Error($"cache retrieval for {cacheKey} timed out. Consider increasing \"syncTimeout\" setting value in redis service configuration section");
					return false;
				}
				catch (Exception e)
				{
					logger.Error(e, $"failed to retrieve cache for {cacheKey} due to an exception");
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
			try
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
			catch (Exception e)
			{
				logger.Error(e, $"failed to invalidate cache with {invalidObject} due to an exception");
			}
		}
	}
}