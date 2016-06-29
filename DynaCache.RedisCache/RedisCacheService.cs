﻿using DynaCache.RedisCache.Internals;
using NLog;
using NLog.Extension;
using System;
using StackExchange.Redis;

namespace DynaCache.RedisCache
{
	public class RedisCacheService : IDynaCacheService
	{
		private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
		private readonly IRedisService _redisService;
		private readonly ICacheSerializer _serializer;

		public RedisCacheService(IRedisService redisService, ICacheSerializer serializer)
		{
			_redisService = redisService;
			_serializer = serializer;
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
	}
}