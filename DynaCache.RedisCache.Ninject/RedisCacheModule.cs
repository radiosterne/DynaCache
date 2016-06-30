using System.Linq;
using DynaCache.RedisCache.Configuration;
using DynaCache.RedisCache.Internals;
using Ninject.Modules;

namespace DynaCache.RedisCache.Ninject
{
	public class RedisCacheModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IDynaCacheService>().To<RedisCacheService>().InSingletonScope();
			Bind<IRedisService>().To<RedisService>().InSingletonScope();
			Bind<ICacheSerializer>().To<ProtobufCacheSerializer>().InSingletonScope();
			Bind<IRedisConfigurationProviderService>().To<RedisConfigurationProviderService>().InSingletonScope();
			Bind<IConcreteKeyPatternProvider>().To<RedisKeyPatternProvider>().WhenInjectedInto<RedisCacheService>().InSingletonScope();
			if (!Kernel.GetBindings(typeof(IInvalidationDescriptor)).Any())
				Bind<IInvalidationDescriptor>().To<EmptyInvalidationDescriptor>().InSingletonScope();
		}
	}
}
