using System.Configuration;

namespace DynaCache.MultilevelCache.Configuration.CacheDispatcher
{
	public class CacheDispatcherConfiguration : ConfigurationSection
	{
		private const string CachingServicesNodeName = "cachingServices";
		private const string CurrentCacheVersionNodeName = "currentCacheVersion";
		private const string PreviousCacheVersionNodeName = "previousCacheVersion";

		[ConfigurationProperty(CachingServicesNodeName)]
		public CachingServices CachingServices => (CachingServices)base[CachingServicesNodeName];

		[ConfigurationProperty(CurrentCacheVersionNodeName, IsRequired = true, IsKey = false)]
		public string CurrentCacheVersion => (string)base[CurrentCacheVersionNodeName];

		[ConfigurationProperty(PreviousCacheVersionNodeName, IsRequired = true, IsKey = false)]
		public string PreviousCacheVersion => (string)base[PreviousCacheVersionNodeName];
	}
}