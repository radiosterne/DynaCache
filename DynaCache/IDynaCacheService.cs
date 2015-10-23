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

	public class MemoryCacheEntry
	{
		public MemoryCacheEntry(object value, int expirationTimeSeconds)
		{
			_expirationTimeSeconds = expirationTimeSeconds;
			Renew(value);
		}

		public MemoryCacheEntry()
		{
			State = CacheServiceEntryState.NotFound;
		}

		public object Value { get; private set; }

		private readonly int _expirationTimeSeconds;

		public DateTime ExpirationTime { get; private set; }

		public CacheServiceEntryState State { get; private set; }

		public bool GetLoadingLock()
		{
			lock (this)
			{
				if (State == CacheServiceEntryState.Loading) return false;

				State = CacheServiceEntryState.Loading;
				return true;
			}
		}

		public void Renew(object value)
		{
			lock (this)
			{
				Value = value;
				ExpirationTime = DateTime.UtcNow.AddSeconds(_expirationTimeSeconds);
				State = CacheServiceEntryState.Actual;
			}
		}

		public void LoadingFailed()
		{
			lock (this)
			{
				State = CacheServiceEntryState.Stale;
			}
		}

		public MemoryCacheEntry EnsureCorrectness()
		{
			lock (this)
			{
				if(State == CacheServiceEntryState.Actual && DateTime.UtcNow > ExpirationTime)
					State = CacheServiceEntryState.Stale;
			}
			return this;
		}
	}

	public enum CacheServiceEntryState
	{
		//Order of elements in this enum is important for creating proxy method
		Actual,
		Loading,
		Stale,
		NotFound
	}
}
