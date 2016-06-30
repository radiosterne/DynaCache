using DynaCache.MultilevelCache.Configuration;
using Ninject.Modules;
using CacheConfigurationProviderService = DynaCache.MultilevelCache.Ninject.Configuration.CacheConfigurationProviderService;

namespace DynaCache.MultilevelCache.Ninject
{
	public class MultilevelCacheModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Unbind<IDynaCacheService>();
			Bind<ICacheConfigurationProviderService>().To<CacheConfigurationProviderService>();
			Bind<IDynaCacheService>().To<MultilevelCacheService>();
		}
	}
}
