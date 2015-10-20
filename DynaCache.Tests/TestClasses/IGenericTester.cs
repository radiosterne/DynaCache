namespace DynaCache.Tests.TestClasses
{
	/// <summary>
	/// A test interface that is used in unit tests to verify that DynaCache works correctly with
	/// generic methods and classes.
	/// </summary>
	/// <typeparam name="TData">The type of the data.</typeparam>
	public interface IGenericTester<TData>
	{
		/// <summary>
		/// Converts the specified data to a string representation.
		/// </summary>
		/// <param name="data">The data to convert.</param>
		/// <returns>The converted data.</returns>
		string Convert(TData data);

		/// <summary>
		/// Tests the specified input.
		/// </summary>
		/// <typeparam name="T">The type of parameter</typeparam>
		/// <param name="input">The input data.</param>
		/// <returns>The result.</returns>
		T Test<T>(T input);
	}
}
