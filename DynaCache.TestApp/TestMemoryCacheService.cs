#region Copyright 2012 Mike Goatly, 2015 Andrey Kurnoskin
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System;

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
		private readonly MemoryCacheService _cache = new MemoryCacheService();

		/// <inheritdoc />
		public virtual MemoryCacheEntry TryGetCachedObject(string cacheKey)
		{
			var entry = _cache.TryGetCachedObject(cacheKey);

			switch (entry.State)
			{
				case CacheServiceEntryState.Actual:
					NotifyState(ConsoleColor.DarkGreen, "Read {0} from cache key {1}", entry.Value, cacheKey);
					break;
				case CacheServiceEntryState.Loading:
					NotifyState(ConsoleColor.DarkYellow, "Read {0} from cache key {1} while new entry is loading", entry.Value, cacheKey);
					break;
				case CacheServiceEntryState.Stale:
					NotifyState(ConsoleColor.DarkBlue, "Read stale entry {0} from cache key {1}", entry.Value, cacheKey);
					break;
				case CacheServiceEntryState.NotFound:
					NotifyState(ConsoleColor.DarkRed, "Cache miss for cache key {0}", cacheKey);
					break;
			}

			return entry;
		}

		/// <summary>
		/// Stores an object in the cache.
		/// </summary>
		/// <param name="cacheKey">The cache key to store the object against.</param>
		/// <param name="data">The data to store against the key.</param>
		/// <param name="duration">The duration, in seconds, to cache the data for.</param>
		public virtual void SetCachedObject(string cacheKey, object data, int duration)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Caching {0} against {1} for {2}s", data, cacheKey, duration);
			Console.ResetColor();
			_cache.SetCachedObject(cacheKey, data, duration);
		}

		private static void NotifyState(ConsoleColor color, string message, params object[] args)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message, args);
			Console.ResetColor();
		}
	}
}
