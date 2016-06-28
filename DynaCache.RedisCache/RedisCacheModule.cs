using DynaCache.RedisCache.Configuration;
using DynaCache.RedisCache.Internals;
using Ninject.Modules;

namespace DynaCache.RedisCache
{
	public class RedisCacheModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IRedisService>().To<RedisService>().InSingletonScope();
			Bind<ICacheSerializer>().To<ProtobufCacheSerializer>().InSingletonScope();
			Bind<IRedisConfigurationProviderService>().To<RedisConfigurationProviderService>();
		}
	}
}
