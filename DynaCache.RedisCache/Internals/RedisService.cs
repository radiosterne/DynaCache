using System;
using DynaCache.RedisCache.Configuration;
using StackExchange.Redis;

namespace DynaCache.RedisCache.Internals
{
	public interface IRedisService
	{
		IDatabase Database { get; }
	}

	// Should be used as a single instance only
	internal class RedisService : IRedisService, IDisposable
	{
		private readonly ConnectionMultiplexer _multiplexer;

		public RedisService(IRedisConfigurationProviderService configurationProviderService)
		{
			var options = configurationProviderService.GetMultiplexorOptions();

			_multiplexer = ConnectionMultiplexer.Connect(options);
		}

		public void Dispose()
			=> _multiplexer.Dispose();

		public IDatabase Database => _multiplexer.GetDatabase();
	}
}