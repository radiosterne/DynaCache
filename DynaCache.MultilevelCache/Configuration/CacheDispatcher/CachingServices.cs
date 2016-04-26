using System.Configuration;

namespace DynaCache.MultilevelCache.Configuration.CacheDispatcher
{
	[ConfigurationCollection(typeof(CachingService), AddItemName = CachingService.CachingServiceNodeName)]
	public class CachingServices : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new CachingService();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((CachingService)element).Name;
		}
	}
}