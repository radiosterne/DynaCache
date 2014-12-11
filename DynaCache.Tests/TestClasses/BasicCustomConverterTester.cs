using System;

namespace DynaCache.Tests.TestClasses
{
    public class BasicCustomConverterTester
    {
        [CacheableMethod(180)]
        public string GetMessage(Exception e)
        {
            return e.Message;
        }
    }
}
