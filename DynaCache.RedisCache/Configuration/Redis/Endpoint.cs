using System.Configuration;

namespace DynaCache.RedisCache.Configuration.Redis
{
	public class Endpoint : ConfigurationElement
	{
		internal const string EndpointNodeName = "endpoint";
		private const string AddressNodeName = "address";
		private const string PortNodeName = "port";

		[ConfigurationProperty(AddressNodeName, IsKey = false, IsRequired = true)]
		public string Address => (string)base[AddressNodeName];

		[ConfigurationProperty(PortNodeName, IsKey = true, IsRequired = false)]
		public string Port => (string)base[PortNodeName];

		public override string ToString()
		{
			return $"{{{nameof(Address)} = {Address}, {nameof(Port)} = {Port}}}";
		}
	}
}