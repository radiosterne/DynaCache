#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System.Collections.Concurrent;

namespace DynaCache
{
	using System;

	/// <summary>
	/// An implementation of <see cref="IDynaCacheService"/> that uses custom in-memory cache.
	/// </summary>
	public class MemoryCacheService : IDynaCacheService, IDisposable
	{
		/// <summary>
		/// An object that represents a cached null value. (MemoryCache does not allow for null values to be cached explicitly.)
		/// </summary>
		private static readonly object NullReference = new object();

		private readonly ConcurrentDictionary<string, MemoryCacheEntry> _table = new ConcurrentDictionary<string, MemoryCacheEntry>();

		/// <inheritdoc />
		public virtual MemoryCacheEntry TryGetCachedObject(string cacheKey)
		{
			return _table.ContainsKey(cacheKey) ? _table[cacheKey] : new MemoryCacheEntry();
		}

		/// <inheritdoc />
		public virtual void SetCachedObject(string cacheKey, object data, int duration)
		{
			if (data == null)
			{
				data = NullReference;
			}

			var entry = new MemoryCacheEntry(data, duration);

			_table[cacheKey] = entry;
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
			}
		}
	}
}
