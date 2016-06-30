using DynaCache.MultilevelCache.Configuration;
using Ninject.Modules;
using CacheConfigurationProviderService = DynaCache.MultilevelCache.Ninject.Configuration.CacheConfigurationProviderService;

namespace DynaCache.MultilevelCache.Ninject
{
	/// <summary>
	/// This module should be loaded after other modules that can implement IDynaCacheService and ICacheInvalidator
	/// </summary>
	public class MultilevelCacheModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Unbind<IDynaCacheService>();
			Kernel.Unbind<ICacheInvalidator>();
			Bind<ICacheConfigurationProviderService>().To<CacheConfigurationProviderService>().InSingletonScope();
			Bind<IDynaCacheService, ICacheInvalidator>().To<MultilevelCacheService>().InSingletonScope();
		}
	}
}
