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
	}

	// Should be used as a single instance only
	internal class RedisService : IRedisService, IDisposable
	{
		private readonly IRedisConfigurationProviderService _configuration;
		private readonly ConnectionMultiplexer _multiplexer;

		public RedisService(IRedisConfigurationProviderService configurationProviderService)
		{
			_configuration = configurationProviderService;
			_multiplexer = ConnectionMultiplexer.Connect(_configuration.GetMultiplexorOptions());
		}

		public void Dispose()
			=> _multiplexer.Dispose();

		public IDatabase Database => _multiplexer.GetDatabase();

		public IReadOnlyCollection<IServer> Servers
			=> _configuration.GetMultiplexorOptions().EndPoints
				.Select(e => _multiplexer.GetServer(e))
				.ToList();
	}
}