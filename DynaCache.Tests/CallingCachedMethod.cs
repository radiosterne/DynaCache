#region Copyright 2012 Mike Goatly, 2015 Andrey Kurnoskin
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.Tests
{
	using System;

	using TestClasses;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Moq;

	/// <summary>
	/// The calling cached method.
	/// </summary>
	[TestClass]
	public class CallingCachedMethod
	{
		/// <summary>
		/// Named cache durations should be used successfully.
		/// </summary>
		[TestMethod]
		public void ShouldReadCorrectConfigurationFromNamedCacheDurations()
		{
			const string keyNameA = "DynaCache_Tests_TestClasses_NamedCacheableMethodTester_System_Object_Execute_System_String__Hello world";
			const string keyNameB = "DynaCache_Tests_TestClasses_NamedCacheableMethodTester_Int32_Execute_System_DateTime__2014-11-01T00:00:00.0000000";

			var cacheService = new Mock<IDynaCacheService>(MockBehavior.Strict);
			var cacheableType = Cacheable.CreateType<NamedCacheableMethodTester>();

			var instance = (ICacheableMethodsTester)Activator.CreateInstance(cacheableType, cacheService.Object);

			cacheService.Setup(s => s.TryGetCachedObject(keyNameA)).Returns(new MemoryCacheEntry());
			cacheService.Setup(s => s.SetCachedObject(keyNameA, It.IsAny<string[]>(), 1));
			cacheService.Setup(s => s.TryGetCachedObject(keyNameB)).Returns(new MemoryCacheEntry());
			cacheService.Setup(s => s.SetCachedObject(keyNameB, It.IsAny<int>(), 60));

			var responseA = instance.Execute("Hello world");
			cacheService.Verify(s => s.TryGetCachedObject(keyNameA), Times.Exactly(1));
			cacheService.Verify(s => s.SetCachedObject(keyNameA, responseA, 1), Times.Exactly(1));
			var responseB = instance.Execute(new DateTime(2014, 11, 1));
			cacheService.Verify(s => s.TryGetCachedObject(keyNameB), Times.Exactly(1));
			cacheService.Verify(s => s.SetCachedObject(keyNameB, responseB, 60), Times.Exactly(1));
		}

		/// <summary>
		/// The first time a method is called, the cache will not contain the key and
		/// the base method should be called - the result of which should be cached.
		/// </summary>
		[TestMethod]
		public void ShouldWriteToCacheServiceOnFirstCall()
		{
			const string keyName = "DynaCache_Tests_TestClasses_OneCacheableMethodTester_System_Object_Execute_System_String__Hello world";

			var cacheService = new Mock<IDynaCacheService>(MockBehavior.Strict);
			var cacheableType = Cacheable.CreateType<OneCacheableMethodTester>();

			var instance = (ICacheableMethodsTester)Activator.CreateInstance(cacheableType, cacheService.Object);

			cacheService.Setup(s => s.TryGetCachedObject(keyName)).Returns(new MemoryCacheEntry());
			cacheService.Setup(s => s.SetCachedObject(keyName, It.IsAny<string[]>(), 100));

			var response = instance.Execute("Hello world");

			cacheService.Verify(s => s.TryGetCachedObject(keyName), Times.Exactly(1));
			cacheService.Verify(s => s.SetCachedObject(keyName, response, 100), Times.Exactly(1));
		}

		/// <summary>
		/// Verifies that a key is created successfully for a method on a generic class.
		/// </summary>
		[TestMethod]
		public void ShouldReadWithCorrectKeyForGenericClass()
		{
			const string keyName = "DynaCache_Tests_TestClasses_GenericTester_1__System_Int32__mscorlib__Version_4_0_0_0__Culture_neutral__PublicKeyToken_b77a5c561934e089___System_String_Convert_Int32__199";

			var cacheService = new Mock<IDynaCacheService>(MockBehavior.Strict);
			var cacheableType = Cacheable.CreateType<GenericTester<int>>();

			var instance = (IGenericTester<int>)Activator.CreateInstance(cacheableType, cacheService.Object);

			const string result = "Blah";
			cacheService.Setup(s => s.TryGetCachedObject(keyName)).Returns(new MemoryCacheEntry(result, 1000, keyName));

			var response = instance.Convert(199);

			Assert.AreSame(response, result);
			cacheService.Verify(s => s.TryGetCachedObject(keyName), Times.Exactly(1));
			cacheService.Verify(s => s.SetCachedObject(keyName, It.IsAny<object>(), It.IsAny<int>()), Times.Never());
		}

		/// <summary>
		/// Parameters passed by reference are not supported and an exception should be thrown.
		/// </summary>
		[TestMethod, ExpectedException(typeof(DynaCacheException))]
		public void ShouldThrowExceptionForMethodWithReferenceParameters()
		{
			Cacheable.CreateType<RefModifierTester>();
		}

		/// <summary>
		/// Nullable parameters should be handled correctly.
		/// </summary>
		[TestMethod]
		public void ShouldCreateValidProxyForNullableParameter()
		{
			var cacheService = new Mock<IDynaCacheService>();
			cacheService.Setup(s => s.TryGetCachedObject(It.IsAny<string>())).Returns(new MemoryCacheEntry());
			var cacheableType = Cacheable.CreateType<NullableReturnTypeMethod>();

			var instance = (INullableReturnTypeMethod)Activator.CreateInstance(cacheableType, cacheService.Object);

			var result = instance.ReturnsNullable(6);

			Assert.AreEqual(6, result);
		}


		/// <summary>
		/// ToStringable parameters should be handled correctly.
		/// </summary>
		[TestMethod]
		public void ShouldCreateValidProxyForToStringableParameter()
		{
			const string testString1 = "TestString1";
			const string testString2 = "TestString2";
			var cacheService = new Mock<IDynaCacheService>();
			cacheService.Setup(s => s.TryGetCachedObject(It.IsAny<string>())).Returns(new MemoryCacheEntry());
			var cacheableType = Cacheable.CreateType<ToStringableTester>();

			var instance = (ToStringableTester)Activator.CreateInstance(cacheableType, cacheService.Object);

			var result = instance.GetToStringableValue(new ToStringableObject { Value = testString1 });

			Assert.AreEqual(testString1, result);

			result = instance.GetToStringableValue(new ToStringableObject { Value = testString2 });

			Assert.AreEqual(testString2, result);
		}

		/// <summary>
		/// Enum parameters should be handled correctly.
		/// </summary>
		[TestMethod]
		public void ShouldCreateValidProxyForEnumParameter()
		{
			const TestEnum testEnum1 = TestEnum.FirstValue;
			const TestEnum testEnum2 = TestEnum.SecondValue;
			var cacheService = new Mock<IDynaCacheService>();
			cacheService.Setup(s => s.TryGetCachedObject(It.IsAny<string>())).Returns(new MemoryCacheEntry());
			var cacheableType = Cacheable.CreateType<EnumTester>();

			var instance = (EnumTester)Activator.CreateInstance(cacheableType, cacheService.Object);

			var result = instance.GetEnumValue(testEnum1);

			Assert.AreEqual(testEnum1.ToString(), result);

			result = instance.GetEnumValue(testEnum2);

			Assert.AreEqual(testEnum2.ToString(), result);
		}
	}
}