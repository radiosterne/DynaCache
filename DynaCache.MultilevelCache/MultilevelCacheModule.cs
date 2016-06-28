using DynaCache.MultilevelCache.Configuration;
using Ninject.Modules;

namespace DynaCache.MultilevelCache
{
	public class MultilevelCacheModule : NinjectModule
	{
		public override void Load()
		{
			Bind<ICacheConfigurationProviderService>().To<CacheConfigurationProviderService>();
		}
	}
}
