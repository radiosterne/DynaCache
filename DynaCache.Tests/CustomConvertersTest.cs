using System;
using DynaCache.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DynaCache.Tests
{
	[TestClass]
	public class CustomConvertersTest
	{
		[TestMethod]
		public void ShouldUseSimpleCustomConverter()
		{
			Func<Exception, string> converter = e => e.Message;

			Cacheable.AddCustomConverter<Exception>(ExceptionConverter);

			const string testString1 = "TestString1";
			const string testString2 = "TestString2";
			var cacheService = new MemoryCacheService();
			var cacheableType = Cacheable.CreateType<BasicCustomConverterTester>();
			Cacheable.SaveAssembly();

			var instance = (BasicCustomConverterTester)Activator.CreateInstance(cacheableType, cacheService);

			var result = instance.GetMessage(new Exception(testString1));

			Assert.AreEqual(testString1, result);

			result = instance.GetMessage(new Exception(testString2));

			Assert.AreEqual(testString2, result);
		}

		public static string ExceptionConverter(Exception e)
		{
			return e.Message;
		}
	}
}
