using System;
using DynaCache.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynaCache.Tests
{
	[TestClass]
	public class CustomConvertersTest
	{
		[TestMethod]
		public void ShouldUseSimpleCustomConverter()
		{
			Func<Exception, string> converter = e => e.Message;

			Cacheable.AddCustomConverter(converter);

			const string testString1 = "TestString1";
			const string testString2 = "TestString2";
			var cacheService = new MemoryCacheService();
			var cacheableType = Cacheable.CreateType<BasicCustomConverterTester>();

			var instance = (BasicCustomConverterTester)Activator.CreateInstance(cacheableType, cacheService);

			var result = instance.GetMessage(new Exception(testString1));

			Assert.AreEqual(testString1, result);

			result = instance.GetMessage(new Exception(testString2));

			Assert.AreEqual(testString2, result);
		}
	}
}
