using System.Configuration;

namespace DynaCache.MultilevelCache.Configuration.CacheDispatcher
{
	public class CachingService : ConfigurationElement
	{
		internal const string CachingServiceNodeName = "cachingService";
		private const string NameNodeName = "name";
		private const string TypeNodeName = "type";
		private const string TimeoutNodeName = "timeout";
		private const string ExpirationNodeName = "expiration";

		[ConfigurationProperty(NameNodeName, IsKey = false, IsRequired = true)]
		public string Name => (string)base[NameNodeName];

		[ConfigurationProperty(TypeNodeName, IsKey = true, IsRequired = true)]
		public string Type => (string)base[TypeNodeName];

		[ConfigurationProperty(TimeoutNodeName, IsKey = false, IsRequired = true)]
		public string Timeout => (string)base[TimeoutNodeName];

		[ConfigurationProperty(ExpirationNodeName, IsKey = false, IsRequired = true)]
		public string Expiration => (string)base[ExpirationNodeName];

		public override string ToString()
		{
			return
				$"{{{nameof(Name)} = {Name}, {nameof(Type)} = {Type}, {nameof(Timeout)} = {Timeout}, {nameof(Expiration)} = {Expiration}}}";
		}
	}
}