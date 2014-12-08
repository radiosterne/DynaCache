#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.Tests
{
    using System;
    using DynaCache.Tests.TestClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for creating a cacheable proxy type.
    /// </summary>
    [TestClass]
    public class CreatingCacheableProxyType
    {
        /// <summary>
        /// The proxy generator should the return same type as the input type if it has no 
        /// methods that are marked as cacheable.
        /// </summary>
        [TestMethod]
        public void ShouldReturnSameTypeIfNoMethodsMarkedAsCacheable()
        {
            var cacheableType = Cacheable.CreateType<NoCacheableMethodsTester>();

            Assert.AreSame(typeof(NoCacheableMethodsTester), cacheableType);
        }

        /// <summary>
        /// The proxy generator should return a proxy type if only one method on the input
        /// type is marked as cacheable.
        /// </summary>
        [TestMethod]
        public void ShouldReturnProxyIfOneMethodMarkedAsCacheable()
        {
            var cacheableType = Cacheable.CreateType<OneCacheableMethodTester>();

            Assert.AreNotSame(typeof(OneCacheableMethodTester), cacheableType);

            Assert.AreSame(typeof(OneCacheableMethodTester), cacheableType.GetMethod("Execute", Type.EmptyTypes).DeclaringType);
            Assert.AreSame(typeof(OneCacheableMethodTester), cacheableType.GetMethod("Execute", new[] { typeof(DateTime) }).DeclaringType);
            Assert.AreSame(cacheableType, cacheableType.GetMethod("Execute", new[] { typeof(string) }).DeclaringType);
            Assert.AreSame(typeof(OneCacheableMethodTester), cacheableType.GetMethod("Execute", new[] { typeof(int), typeof(object) }).DeclaringType);
        }

        /// <summary>
        /// The proxy generator should not generate duplicate proxies for a type - if
        /// a proxy is requested for a type that a proxy has already been generated for,
        /// then the first type should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldReturnSameProxySameTypeRequestedMutipleTimes()
        {
            var cacheableType1 = Cacheable.CreateType<OneCacheableMethodTester>();
            var cacheableType2 = Cacheable.CreateType<OneCacheableMethodTester>();

            Assert.AreSame(cacheableType1, cacheableType2);
        }
    }
}
