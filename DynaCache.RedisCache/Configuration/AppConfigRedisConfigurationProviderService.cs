using System.Configuration;
using System.Linq;
using DynaCache.RedisCache.Configuration.Redis;
using DynaCache.RedisCache.Internals;
using Functional.Maybe;
using NLog;
using StackExchange.Redis;

namespace DynaCache.RedisCache.Configuration
{
	internal class AppConfigRedisConfigurationProviderService : IRedisConfigurationProviderService
	{
		private const string RedisSectionName = "redis";

		private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

		private readonly RedisConfiguration _redisSection;

		public AppConfigRedisConfigurationProviderService()
		{
			_redisSection = (RedisConfiguration)ConfigurationManager.GetSection(RedisSectionName);
		}

		public ConfigurationOptions GetMultiplexorOptions()
		{
			var res = new ConfigurationOptions();
			_redisSection.Endpoints
				.Cast<Endpoint>()
				.Select(e => new {Host = e.Address, PortMaybe = e.Port.ParseMaybe<ushort>(ushort.TryParse), PortRaw = e.Port})
				.ToList()
				.Select(e => new {e.Host, PortMaybe = e.PortMaybe.Match(p => { }, () => logger.Error($"failed to parse {e.PortRaw} as ip port"))})
				.Where(e => e.PortMaybe.HasValue)
				.ToList()
				.ForEach(e => res.EndPoints.Add(e.Host, e.PortMaybe.Value));

			_redisSection.SyncTimeout.ParseMaybe<int>(int.TryParse).Do(t => res.SyncTimeout = t);

			return res;
		}
	}
}