using System.Collections.Generic;

namespace DynaCache.TestApp.CustomConverters
{
	public static class CacheConverters
	{
		public static string TestConvert(Test test)
		{
			return test.Kekos.ToString();
		}

		public static string ListConvert<T>(List<T> list)
		{
			return string.Join(",", list);
		}
	}
}