
namespace DynaCache.Tests.TestClasses
{
	public class ToStringableTester
	{
		[CacheableMethod(200)]
		public virtual string GetToStringableValue(ToStringableObject obj)
		{
			return obj.Value;
		}
	}

	[ToStringable]
	public class ToStringableObject
	{
		public string Value { get; set; }

		public override string ToString()
		{
 			 return Value;
		}
	}
}
