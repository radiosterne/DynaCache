namespace DynaCache
{
	/// <summary>
	/// This 
	/// </summary>
	public interface ICacheInvalidator
	{
		/// <summary>
		/// Invalidates all cache stored with the given keys
		/// </summary>
		/// <param name="invalidObject">
		/// Object that no longer is valid (and therefore every piece of cached data that is related to
		/// this object is neither)</param>
		void InvalidateCache(object invalidObject);
	}
}
