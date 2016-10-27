namespace DynaCache.Tests.TestClasses
{
	/// <summary>
	/// A test class that is used in unit tests to verify that DynaCache works correctly with
	/// classes with constructor
	/// </summary>
	public class ConstructorTester
	{
		private readonly string _param;
		public ConstructorTester(string param)
		{
			_param = param;
		}

		[CacheableMethod(200)]
		public virtual string GetParam() => _param;
	}
}
