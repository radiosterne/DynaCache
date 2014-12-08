namespace DynaCache.Tests.TestClasses
{
    /// <summary>
    /// A test class that is used in unit tests to verify that DynaCache works correctly with
    /// generic methods and classes.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class GenericTester<TData> : IGenericTester<TData>
    {
        /// <summary>
        /// Converts the specified data to a string representation.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <returns>The converted data.</returns>
        [CacheableMethod(200)]
        public virtual string Convert(TData data)
        {
            return data.ToString();
        }

        /// <summary>
        /// Tests the specified input.
        /// </summary>
        /// <typeparam name="T">The type of parameter</typeparam>
        /// <param name="input">The input data.</param>
        /// <returns>The result.</returns>
        [CacheableMethod(300)]
        public virtual T Test<T>(T input)
        {
            return input;
        }
    }
}