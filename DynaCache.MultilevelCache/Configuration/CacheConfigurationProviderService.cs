using DynaCache.MultilevelCache.Configuration.CacheDispatcher;
using DynaCache.MultilevelCache.Domain;
using DynaCache.MultilevelCache.Internals;
using Functional.Maybe;
using NLog;
using NLog.Extension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DynaCache.MultilevelCache.Configuration
{
	public class CacheConfigurationProviderService : ICacheConfigurationProviderService
	{
		private const string CacheDispatcherSectionName = "dynaCache.multilevelCache";

		private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
		private readonly CacheDispatcherConfiguration _cdSection;

		private readonly IDynaCacheService[] _leveledCacheServiceImplementations;

		public CacheConfigurationProviderService(IDynaCacheService[] leveledCacheServiceImplementations)
		{
			_cdSection = (CacheDispatcherConfiguration)ConfigurationManager.GetSection(CacheDispatcherSectionName);
			_leveledCacheServiceImplementations = leveledCacheServiceImplementations;
		}

		public uint GetCurrentCacheVersion()
			=> ParseCacheVersion(_cdSection.CurrentCacheVersion, nameof(_cdSection.CurrentCacheVersion));

		public uint GetPreviousCacheVersion()
			=> ParseCacheVersion(_cdSection.PreviousCacheVersion, nameof(_cdSection.PreviousCacheVersion));

		public IReadOnlyCollection<CacheServiceContext> GetCachingServices()
		{
			return _cdSection.CachingServices
				.Cast<CachingService>()
				.Select(ParseCachingService)
				.WhereValueExist()
				.ToList();
		}

		private Maybe<CacheServiceContext> ParseCachingService(CachingService raw)
		{
			using (new TracingLogProxy(logger))
			{
				var errors = new StringBuilder();
				var instance = _leveledCacheServiceImplementations
					.Where(i => i.GetType().FullName == raw.Type)
					.FirstMaybe();
				if (!instance.HasValue)
					errors.AppendLine($"cannot find type with name {raw.Type} among implemented {nameof(IDynaCacheService)}");
				var lifeSpan = raw.Expiration.ParseMaybe<long>(long.TryParse);
				if (!lifeSpan.HasValue)
					errors.AppendLine($"failed to parse {nameof(raw.Expiration)} value {raw.Expiration}");
				var timeout = raw.Timeout.ParseMaybe<long>(long.TryParse);
				if (!timeout.HasValue)
					errors.AppendLine($"failed to parse {nameof(raw.Timeout)} value {raw.Timeout}");
				if (errors.Length == 0)
					return new CacheServiceContext
					{
						Name = raw.Name,
						ServiceInstance = instance.Value,
						RetrievalTimeout = TimeSpan.FromMilliseconds(timeout.Value),
						CacheLifeSpan = TimeSpan.FromMilliseconds(lifeSpan.Value)
					}.ToMaybe();
				logger.Error($"failed to parse {raw.Name} configuration entry due to following errors:{Environment.NewLine}{errors}");
				return Maybe<CacheServiceContext>.Nothing;
			}
		}

		private static uint ParseCacheVersion(string entryValue, string entryName)
		{
			using (new TracingLogProxy(logger))
			{
				uint res;
				if (!uint.TryParse(entryValue, out res))
					logger.Error($"failed to parse {entryName} value {entryValue} as uint");
				return res;
			}
		}
	}
}