namespace DynaCache.RedisCache
{
	public class RedisKeyPatternProvider : IConcreteKeyPatternProvider
	{
		public string ConvertCommonKey(string commonKey)
			=> commonKey;
	}
}
