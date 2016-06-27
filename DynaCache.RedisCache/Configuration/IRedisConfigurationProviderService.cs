using StackExchange.Redis;

namespace DynaCache.RedisCache.Configuration
{
	public interface IRedisConfigurationProviderService
	{
		ConfigurationOptions GetMultiplexorOptions();
	}
}