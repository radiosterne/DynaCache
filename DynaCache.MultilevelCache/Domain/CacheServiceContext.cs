using System;

namespace DynaCache.MultilevelCache.Domain
{
	public class CacheServiceContext
	{
		public string Name { get; set; }
		public IDynaCacheService ServiceInstance { get; set; }
		public TimeSpan RetrievalTimeout { get; set; }
		public TimeSpan CacheLifeSpan { get; set; }
	}
}