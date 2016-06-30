using Ninject;
using Base = DynaCache.MultilevelCache.Configuration.CacheConfigurationProviderService;

namespace DynaCache.MultilevelCache.Ninject.Configuration
{
	public class CacheConfigurationProviderService : Base
	{
		private const string LeveledCacheNaming = "LeveledCache";

		public CacheConfigurationProviderService([Named(LeveledCacheNaming)] IDynaCacheService[] leveledCacheServiceImplementations)
			: base(leveledCacheServiceImplementations)
		{ }
	}
}
