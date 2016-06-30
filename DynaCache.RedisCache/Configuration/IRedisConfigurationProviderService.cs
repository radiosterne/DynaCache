using StackExchange.Redis;

namespace DynaCache.RedisCache.Configuration
{
	public interface IRedisConfigurationProviderService
	{
		int GetPollingPageSize();

		ConfigurationOptions GetMultiplexorOptions();
	}
}