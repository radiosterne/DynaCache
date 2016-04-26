using System.Configuration;
using System.Text;

namespace DynaCache.RedisCache.Configuration.Redis
{
	[ConfigurationCollection(typeof(Endpoint), AddItemName = Endpoint.EndpointNodeName)]
	public class Endpoints : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new Endpoint();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			var e = (Endpoint)element;
			var builder = new StringBuilder();
			builder.Append(e.Address);
			if (e.Port != null)
				builder.Append(':').Append(e.Port);
			return builder.ToString();
		}
	}
}