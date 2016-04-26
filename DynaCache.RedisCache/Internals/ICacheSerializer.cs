namespace DynaCache.RedisCache.Internals
{
	public interface ICacheSerializer
	{
		string Serialize<T>(T @object);
		T Deserialize<T>(string @object);
	}
}