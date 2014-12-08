#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
    using System;
    using System.Runtime.Caching;

    /// <summary>
    /// An implementation of <see cref="IDynaCacheService"/> that uses the .NET 4.0 in-memory cache.
    /// </summary>
    public class MemoryCacheService : IDynaCacheService, IDisposable
    {
        /// <summary>
        /// An object that represents a cached null value. (MemoryCache does not allow for null values to be cached explicitly.)
        /// </summary>
        private static readonly object NullReference = new object();

        /// <summary>
        /// The in-memory cache instance for this service.
        /// </summary>
        private readonly MemoryCache cache = new MemoryCache("CacheService");

        /// <summary>
        /// Tries to get a cached object from the cache using the given cache key.
        /// </summary>
        /// <param name="cacheKey">The cache key of the object to read from the cache.</param>
        /// <param name="result">The object that was read from the cache, or null if the key
        /// could not be found in the cache.</param>
        /// <returns><c>true</c> if the item could be read from the cache, otherwise <c>false</c>.</returns>
        public virtual bool TryGetCachedObject(string cacheKey, out object result)
        {
            if (this.cache.Contains(cacheKey))
            {
                result = this.cache[cacheKey];
                if (object.Equals(result, NullReference))
                {
                    result = null;
                }

                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Stores an object in the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key to store the object against.</param>
        /// <param name="data">The data to store against the key.</param>
        /// <param name="duration">The duration, in seconds, to cache the data for.</param>
        public virtual void SetCachedObject(string cacheKey, object data, int duration)
        {
            if (data == null)
            {
                data = NullReference;
            }

            this.cache.Add(cacheKey, data, DateTime.Now.AddSeconds(duration));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
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
                this.cache.Dispose();
            }
        }
    }
}
