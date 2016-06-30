using System;
using System.Collections.Generic;
using System.Linq;
using DynaCache.RedisCache.Configuration;
using StackExchange.Redis;

namespace DynaCache.RedisCache.Internals
{
	public interface IRedisService
	{
		IDatabase Database { get; }
		IReadOnlyCollection<IServer> Servers { get; } 
		IRedisConfigurationProviderService Configuration { get; }
	}

	// Should be used as a single instance only
	public class RedisService : IRedisService, IDisposable
	{
		private readonly ConnectionMultiplexer _multiplexer;

		public RedisService(IRedisConfigurationProviderService configurationProviderService)
		{
			Configuration = configurationProviderService;
			_multiplexer = ConnectionMultiplexer.Connect(Configuration.GetMultiplexorOptions());
		}

		public void Dispose()
			=> _multiplexer.Dispose();

		public IDatabase Database => _multiplexer.GetDatabase();

		public IReadOnlyCollection<IServer> Servers
			=> Configuration.GetMultiplexorOptions().EndPoints
				.Select(e => _multiplexer.GetServer(e))
				.ToList();

		public IRedisConfigurationProviderService Configuration { get; }
	}
}