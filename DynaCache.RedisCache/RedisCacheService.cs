using DynaCache.RedisCache.Internals;
using NLog;
using StackExchange.Redis;
using System;
using System.Linq;

namespace DynaCache.RedisCache
{
	public class RedisCacheService : IDynaCacheService, ICacheInvalidator
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
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
				logger.Error($"failed to retrieve cache for {cacheKey} due to {e}");
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
				logger.Error($"failed to deserialize cache for {cacheKey} due to {e}");
				return false;
			}
		}

		public void SetCachedObject<T>(string cacheKey, T data, int duration)
		{
			var expiration = TimeSpan.FromSeconds(duration);
			logger.Debug($"storing cache for {cacheKey} with expiry {expiration}");
			string serialized;
			try
			{
				serialized = _serializer.Serialize(data);
			}
			catch (Exception e)
			{
				logger.Error($"failed to serialize {data} for key {cacheKey} due to {e}");
				return;
			}
			try
			{
				_redisService.Database.StringSet(cacheKey, serialized, expiration);
			}
			catch (TimeoutException)
			{
				logger.Error($"cache setter for {cacheKey} timed out. Consider increasing \"syncTimeout\" setting value in redis service configuration section");
			}
			catch (Exception e)
			{
				logger.Error($"failed to set cache for {cacheKey} due to {e}");
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
				if (keys.Length != deleted)
					logger.Warn($"Redis found {keys.Length}, but deleted only {deleted}");
			}
			catch (Exception e)
			{
				logger.Error($"failed to invalidate cache with {invalidObject} due to an exception due to {e}");
			}
		}
	}
}