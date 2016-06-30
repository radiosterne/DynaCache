using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DynaCache.MultilevelCache.Configuration;
using DynaCache.MultilevelCache.Domain;
using DynaCache.MultilevelCache.Internals;
using Functional.Maybe;
using NLog;
using NLog.Extension;

namespace DynaCache.MultilevelCache
{
	public class MultilevelCacheService : IDynaCacheService, ICacheInvalidator
	{
		private const string CacheVersionPrefix = "cache-version-";

		private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

		private readonly IReadOnlyCollection<CacheServiceContext> _cacheServices;
		private readonly uint _currentCacheVersion;
		private readonly uint _previousCacheVersion;

		public MultilevelCacheService(ICacheConfigurationProviderService configurationProviderService)
		{
			_cacheServices = configurationProviderService.GetCachingServices();
			_currentCacheVersion = configurationProviderService.GetCurrentCacheVersion();
			_previousCacheVersion = configurationProviderService.GetPreviousCacheVersion();
		}
		
		public bool TryGetCachedObject<T>(string cacheKey, out T result)
		{
			var retrieved = RetrieveMaybe<T>(cacheKey, _currentCacheVersion) // try to get value from current cache version
				.Or(() =>
				{
					var res = RetrieveMaybe<T>(cacheKey, _previousCacheVersion); // try to get value from previous cache version
					res.Do(v => Store(cacheKey, _currentCacheVersion, v)); // save to new version if old version fits new
					return res;
				});
			result = retrieved.HasValue ? retrieved.Value : default(T);
			return retrieved.HasValue;
		}

		public void SetCachedObject<T>(string cacheKey, T data, int duration)
			=> Store(cacheKey, _currentCacheVersion, data, TimeSpan.FromSeconds(duration));

		public void InvalidateCache(object invalidObject)
			=> _cacheServices
				.Select(cs => cs.ServiceInstance)
				.OfType<ICacheInvalidator>()
				.ToList().ForEach(ici => ici.InvalidateCache(invalidObject));

		private Maybe<T> RetrieveMaybe<T>(string cacheKey, uint cacheVersion)
		{
			using (new TracingLogProxy(logger))
			{
				var emptyCacheServices = new List<CacheServiceContext>();
				var versionedCacheKey = ApplyVersionToKey(cacheKey, cacheVersion);
				logger.Debug($"requested value for {versionedCacheKey} key");
				foreach (var cacheServiceContext in _cacheServices)
				{
					var cts = new CancellationTokenSource();
					var pending = Task.Run(() => versionedCacheKey.ParseMaybe<T>(cacheServiceContext.ServiceInstance.TryGetCachedObject), cts.Token);
					try
					{
						var timedOut = !pending.Wait(cacheServiceContext.RetrievalTimeout);
						if (timedOut)
						{
							cts.Cancel();
							logger.Warn($"cache request for {versionedCacheKey} at {cacheServiceContext.Name} has timed out");
							emptyCacheServices.Add(cacheServiceContext);
							continue;
						}
					}
					catch (Exception e)
					{
						logger.Error(e, $"cache request for {versionedCacheKey} at {cacheServiceContext.Name} has faulted");
						emptyCacheServices.Add(cacheServiceContext);
						continue;
					}
					var res = pending.Result;
					if (res.HasValue)
					{
						emptyCacheServices.ForEach(ecs => ecs.ServiceInstance.SetCachedObject(versionedCacheKey, res.Value, ecs.CacheLifeSpan.Seconds));
						logger.Debug($"cache for {versionedCacheKey} key found at {cacheServiceContext.Name}");
						return res;
					}
					logger.Debug($"cache not found for {versionedCacheKey} at {cacheServiceContext.Name}");
					emptyCacheServices.Add(cacheServiceContext);
				}
				logger.Debug($"failed to find cached value for {versionedCacheKey}");
				return Maybe<T>.Nothing;
			}
		}

		private void Store<T>(string cacheKey, uint cacheVersion, T value, TimeSpan? lastServiceExpiration = null)
		{
			using (new TracingLogProxy(logger))
			{
				var versionedCacheKey = ApplyVersionToKey(cacheKey, cacheVersion);
				logger.Debug($"storing value for key {versionedCacheKey} in cache");
				using (var cacheServiceIterator = _cacheServices.GetEnumerator())
				{
					if (!cacheServiceIterator.MoveNext())
						return;
					var cs = cacheServiceIterator.Current;
					while (cacheServiceIterator.MoveNext())
					{
						cs.ServiceInstance.SetCachedObject(versionedCacheKey, value, RoundSeconds(cs.CacheLifeSpan));
						cs = cacheServiceIterator.Current;
					}
					cs.ServiceInstance.SetCachedObject(versionedCacheKey, value, RoundSeconds(lastServiceExpiration ?? cs.CacheLifeSpan));
				}
			}
		}

		private static int RoundSeconds(TimeSpan timeSpan)
		{
			var seconds = timeSpan.TotalSeconds;
			return
				int.MaxValue < seconds
					? int.MaxValue
					: Convert.ToInt32(seconds);

		}

		private static string ApplyVersionToKey(string cacheKey, uint cacheVersion)
		{
			using (new TracingLogProxy(logger))
				return Regex.IsMatch(cacheKey, $@"{Regex.Escape(CacheVersionPrefix)}\d+$")
					? cacheKey
					: $"{CacheVersionPrefix}{cacheVersion}:" + cacheKey;
		}
	}
}