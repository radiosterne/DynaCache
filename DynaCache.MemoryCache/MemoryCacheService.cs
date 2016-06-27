#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System;
using MemCache = System.Runtime.Caching.MemoryCache;

namespace DynaCache.MemoryCache
{
	/// <summary>
	/// An implementation of <see cref="IDynaCacheService"/> that uses the .NET 4.0 in-memory cache.
	/// </summary>
	public class MemoryCacheService : IDynaCacheService, IDisposable
	{
		/// <summary>
		/// An object that represents a cached null value. (MemoryCache does not allow for null values to be cached explicitly.)
		/// </summary>
		private static readonly object nullReference = new object();

		/// <summary>
		/// The in-memory cache instance for this service.
		/// </summary>
		private readonly MemCache _cache = new MemCache("CacheService");

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
			if (!_cache.Contains(cacheKey))
				return false;
			var res = _cache[cacheKey];
			if (Equals(res, nullReference))
				return true;
			if (!(res is T))
				return false;
			result = (T)res;
			return true;
		}

		/// <summary>
		/// Stores an object in the cache.
		/// </summary>
		/// <param name="cacheKey">The cache key to store the object against.</param>
		/// <param name="data">The data to store against the key.</param>
		/// <param name="duration">The duration, in seconds, to cache the data for.</param>
		public virtual void SetCachedObject<T>(string cacheKey, T data, int duration)
		{
			// ReSharper disable once ConvertIfStatementToNullCoalescingExpression type conflict
			if (data == null)
				_cache.Add(cacheKey, nullReference, DateTime.Now.AddSeconds(duration));
			else
				_cache.Add(cacheKey, data, DateTime.Now.AddSeconds(duration));
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cache.Dispose();
			}
		}
	}
}
