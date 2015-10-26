#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System;

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
		/// <returns><c>true</c> if the item could be read from the cache and it's not stale, otherwise <c>false</c>.</returns>
		MemoryCacheEntry TryGetCachedObject(string cacheKey);

		/// <summary>
		/// Stores an object in the cache.
		/// </summary>
		/// <param name="cacheKey">The cache key to store the object against.</param>
		/// <param name="data">The data to store against the key.</param>
		/// <param name="duration">The duration, in seconds, to cache the data for.</param>
		void SetCachedObject(string cacheKey, object data, int duration);
	}

	/// <summary>
	/// Internal representation of value stored in cache
	/// </summary>
	/// <remarks>
	/// The only reason it's not, in fact, internal, is because it's used in
	/// dynamic built library
	/// </remarks>
	public class MemoryCacheEntry
	{
		/// <summary>
		/// Creates new cache entry with expriation timeout and specified value, in Actual state
		/// </summary>
		/// <param name="value">Object to cache.</param>
		/// <param name="expirationTimeSeconds">Caching expiration timeout.</param>
		public MemoryCacheEntry(object value, int expirationTimeSeconds)
		{
			_expirationTimeSeconds = expirationTimeSeconds;
			Renew(value);
		}

		/// <summary>
		/// Creates new cache entry denoting a non-existent cache entry
		/// </summary>
		public MemoryCacheEntry()
		{
			State = CacheServiceEntryState.NotFound;
		}

		/// <summary>
		/// Cached value
		/// </summary>
		public object Value { get; private set; }

		private readonly int _expirationTimeSeconds;

		private DateTime _expirationTime;

		/// <summary>
		/// Current entry state
		/// </summary>
		public CacheServiceEntryState State { get; private set; }

		/// <summary>
		/// Gets an exclusive lock to allow asynchronous loading of a new value
		/// </summary>
		/// <returns></returns>
		public bool GetLoadingLock()
		{
			lock (this)
			{
				if (State == CacheServiceEntryState.Loading) return false;

				State = CacheServiceEntryState.Loading;
				return true;
			}
		}

		/// <summary>
		/// Sets new value
		/// </summary>
		/// <param name="value"></param>
		public void Renew(object value)
		{
			lock (this)
			{
				Value = value;
				_expirationTime = DateTime.UtcNow.AddSeconds(_expirationTimeSeconds);
				State = CacheServiceEntryState.Actual;
			}
		}

		/// <summary>
		/// Drops exclusive loading lock and makes entry available for re-loading again
		/// </summary>
		public void LoadingFailed()
		{
			lock (this)
			{
				State = CacheServiceEntryState.Stale;
			}
		}

		/// <summary>
		/// Changes entry state based on expiration timeout
		/// </summary>
		/// <returns>An entry on which it was called.</returns>
		public MemoryCacheEntry EnsureCorrectness()
		{
			lock (this)
			{
				if (State == CacheServiceEntryState.Actual && DateTime.UtcNow > _expirationTime)
					State = CacheServiceEntryState.Stale;
			}
			return this;
		}
	}

	/// <summary>
	/// Possible cache entry states
	/// </summary>
	/// /// <remarks>
	/// Order of elements in this enum is important for creating proxy method
	/// </remarks>
	public enum CacheServiceEntryState
	{
		/// <summary>
		/// Cache entry is not expired yet
		/// </summary>
		Actual = 0,
		/// <summary>
		/// New value loading task has been launched, but has not been completed
		/// </summary>
		Loading = 1,
		/// <summary>
		/// Cache entry expired, but no one launched loading yet
		/// </summary>
		Stale = 2,
		/// <summary>
		/// Cache miss
		/// </summary>
		NotFound = 3
	}
}
