using System.Collections.Generic;
using DynaCache.MultilevelCache.Domain;

namespace DynaCache.MultilevelCache.Configuration
{
	public interface ICacheConfigurationProviderService
	{
		uint GetCurrentCacheVersion();
		uint GetPreviousCacheVersion();
		IReadOnlyCollection<CacheServiceContext> GetCachingServices();
	}
}