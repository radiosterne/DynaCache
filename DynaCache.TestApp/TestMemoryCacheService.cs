#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System;
using System.Runtime.Caching;

namespace DynaCache.TestApp
{
	/// <summary>
	/// A sample memory cache that additionally outputs to the console window when data is stored
	/// and read from the cache.
	/// </summary>
	public class TestMemoryCacheService : IDynaCacheService
	{
		/// <summary>
		/// The in-memory cache.
		/// </summary>
		private readonly MemoryCache _cache = new MemoryCache("CacheService");

		/// <summary>
		/// Tries to get a cached object from the cache using the given cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key of the object to read from the cache.</param>
		/// <param name="result">The object that was read from the cache, or null if the key
		/// could not be found in the cache.</param>
		/// <returns><c>true</c> if the item could be read from the cache, otherwise <c>false</c>.</returns>
		public virtual bool TryGetCachedObject<T>(string cacheKey, out T result)
		{
			result = default(T);
			if (_cache.Contains(cacheKey))
			{
				var res = _cache[cacheKey];
				if (!(res is T))
				{
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("invalid cached data type for key {0}", cacheKey);
					return false;
				}
				result = (T)_cache[cacheKey];
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Read {0} from cache key {1}", result, cacheKey);
				Console.ResetColor();
				return true;
			}

			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("Cache miss for cache key {0}", cacheKey);
			return false;
		}

		/// <summary>
		/// Stores an object in the cache.
		/// </summary>
		/// <param name="cacheKey">The cache key to store the object against.</param>
		/// <param name="data">The data to store against the key.</param>
		/// <param name="duration">The duration, in seconds, to cache the data for.</param>
		public virtual void SetCachedObject<T>(string cacheKey, T data, int duration)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Caching {0} against {1} for {2}s", data, cacheKey, duration);
			Console.ResetColor();
			_cache.Add(cacheKey, data, DateTime.Now.AddSeconds(duration));
		}
	}
}
