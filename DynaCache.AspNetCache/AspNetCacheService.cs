#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.AspNetCache
{
	using System;
	using System.Web;
	using System.Web.Caching;

	/// <summary>
	/// An implementation of <see cref="IDynaCacheService"/> that uses the ASP.NET cache.
	/// </summary>
	public class AspNetCacheService : IDynaCacheService
	{
		/// <summary>
		/// Gets the ASP.NET cache associated to the HttpRuntime.
		/// </summary>
		public static Cache Cache
		{
			get
			{
				var cache = HttpRuntime.Cache;
				if (cache == null)
				{
					throw new InvalidOperationException("No cache available on the HttpRuntime.");
				}

				return cache;
			}
		}

		/// <summary>
		/// Tries to get a cached object from the cache using the given cache key.
		/// </summary>
		/// <param name="cacheKey">The cache key of the object to read from the cache.</param>
		/// <param name="result">The object that was read from the cache, or null if the key
		/// could not be found in the cache.</param>
		/// <returns><c>true</c> if the item could be read from the cache, otherwise <c>false</c>.</returns>
		public virtual bool TryGetCachedObject<T>(string cacheKey, out T result)
		{
			var sample = Cache[cacheKey];
			result = (sample is T)
				? (T)sample
				: default(T);
			return result != null;
		}

		/// <summary>
		/// Stores an object in the cache.
		/// </summary>
		/// <param name="cacheKey">The cache key to store the object against.</param>
		/// <param name="data">The data to store against the key.</param>
		/// <param name="duration">The duration, in seconds, to cache the data for.</param>
		public virtual void SetCachedObject<T>(string cacheKey, T data, int duration)
		{
			Cache.Insert(cacheKey, data, null, DateTime.Now.AddSeconds(duration), Cache.NoSlidingExpiration);
		}
	}
}
