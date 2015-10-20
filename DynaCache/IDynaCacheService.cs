#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
	/// <summary>
	/// The interface implemented by classes capable of acting as a backing cache that
	/// the DynaCache framework will read and write to. This could be implemented using 
	/// a .NET 4.0 memory cache, the ASP.NET web cache, or some other custom cache implementation.
	/// </summary>
	public interface IDynaCacheService
	{
		/// <summary>
		/// Tries to get a cached object from the cache using the given cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key of the object to read from the cache.</param>
		/// <param name="result">The object that was read from the cache, or null if the key
		/// could not be found in the cache.</param>
		/// <returns><c>true</c> if the item could be read from the cache, otherwise <c>false</c>.</returns>
		bool TryGetCachedObject(string cacheKey, out object result);

		/// <summary>
		/// Stores an object in the cache.
		/// </summary>
		/// <param name="cacheKey">The cache key to store the object against.</param>
		/// <param name="data">The data to store against the key.</param>
		/// <param name="duration">The duration, in seconds, to cache the data for.</param>
		void SetCachedObject(string cacheKey, object data, int duration);
	}
}
