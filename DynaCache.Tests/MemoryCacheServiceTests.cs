#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using DynaCache.MemoryCache;

namespace DynaCache.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Threading;

	/// <summary>
	/// Tests for the MemoryCacheService class.
	/// </summary>
	[TestClass]
	public class MemoryCacheServiceTests
	{
		/// <summary>
		/// If an item is not in the cache when TryGetCachedObject is called, false should be returned.
		/// </summary>
		[TestMethod]
		public void ShouldReturnFalseIfItemNotInCache()
		{
			var cache = new MemoryCacheService();

			object result;
			Assert.IsFalse(cache.TryGetCachedObject("key1", out result));
			Assert.IsNull(result);
		}

		/// <summary>
		/// If an item is in the cache when TryGetCachedObject is called, true should be returned
		/// and the result parameter should be set.
		/// </summary>
		[TestMethod]
		public void ShouldReturnTrueIfItemInCache()
		{
			var cache = new MemoryCacheService();

			cache.SetCachedObject("key2", "Boom", 1);

			object result;
			Assert.IsTrue(cache.TryGetCachedObject("key2", out result));
			Assert.AreEqual("Boom", result);
		}

		/// <summary>
		/// If an item is not in the cache when TryGetCachedObject is called because it has expired, false should be 
		/// returned.
		/// </summary>
		[TestMethod]
		public void ShouldReturnFalseIfItemInCacheExpired()
		{
			var cache = new MemoryCacheService();

			cache.SetCachedObject("key3", "Boom", 1);

			Thread.Sleep(1200);

			object result;
			Assert.IsFalse(cache.TryGetCachedObject("key3", out result));
			Assert.IsNull(result);
		}

		/// <summary>
		/// The memory cache should successfully cache null values.
		/// </summary>
		[TestMethod]
		public void ShouldCacheNullValues()
		{
			var cache = new MemoryCacheService();

			cache.SetCachedObject<object>("key3", null, 1);

			object result;
			Assert.IsTrue(cache.TryGetCachedObject("key3", out result));
			Assert.IsNull(result);
		}
	}
}
