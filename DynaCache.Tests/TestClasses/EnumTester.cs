
namespace DynaCache.Tests.TestClasses
{
    public class EnumTester
    {
        [CacheableMethod(200)]
        public virtual string GetEnumValue(TestEnum obj)
        {
            return obj.ToString();
        }
    }

    public enum TestEnum
    {
        FirstValue,
        SecondValue
    }
}
