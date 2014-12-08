#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.Tests.TestClasses
{
    using System;

    /// <summary>
    /// A test interface that is used in unit tests to verify that DynaCache works correctly with
    /// methods that have ref and out parameters.
    /// </summary>
    [CLSCompliant(false)]
    public interface IRefModifierTester
    {
        /// <summary>
        /// Does something with the data, returning the result.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <returns>The converted data.</returns>
        int DoSomething(int data);

        /// <summary>
        /// Tests the specified input.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <returns>
        /// The converted data.
        /// </returns>
        int DoSomething(ref int data);
    }
}
