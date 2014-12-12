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
        public void TestBasicCustomConverter()
        {
            Func<object, string> converter = e => ((Exception)e).Message;

            Cacheable.AddCustomConverter<Exception>(converter);

            const string testString1 = "TestString1";
            const string testString2 = "TestString2";
            var cacheService = new Mock<IDynaCacheService>();
            var cacheableType = Cacheable.CreateType<BasicCustomConverterTester>();

            var instance = (BasicCustomConverterTester)Activator.CreateInstance(cacheableType, cacheService.Object);

            var result = instance.GetMessage(new Exception(testString1));

            Assert.AreEqual(testString1, result);

            result = instance.GetMessage(new Exception(testString2));

            Assert.AreEqual(testString2, result);
        }
    }
}
