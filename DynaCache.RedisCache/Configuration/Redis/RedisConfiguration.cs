using System.Configuration;

namespace DynaCache.RedisCache.Configuration.Redis
{
	public class RedisConfiguration : ConfigurationSection
	{
		private const string EndpointsNodeName = "endpoints";
		private const string SyncTimeoutNodeName = "syncTimeout";

		[ConfigurationProperty(EndpointsNodeName)]
		public Endpoints Endpoints => (Endpoints)base[EndpointsNodeName];

		[ConfigurationProperty(SyncTimeoutNodeName)]
		public string SyncTimeout => (string)base[SyncTimeoutNodeName];
	}
}