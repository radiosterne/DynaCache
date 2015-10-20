#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.Tests.TestClasses
{
	using System;

	/// <summary>
	/// A test class that is used in unit tests to verify that DynaCache works correctly with
	/// methods that have ref and out parameters.
	/// </summary>
	[CLSCompliant(false)]
	public class RefModifierTester : IRefModifierTester
	{
		/// <summary>
		/// Does something with the data, returning the result.
		/// </summary>
		/// <param name="data">The data to convert.</param>
		/// <returns>The converted data.</returns>
		[CacheableMethod(200)]
		public virtual int DoSomething(int data)
		{
			return data * 20;
		}

		/// <summary>
		/// Tests the specified input.
		/// </summary>
		/// <param name="data">The data to convert.</param>
		/// <returns>
		/// The converted data.
		/// </returns>
		[CacheableMethod(300)]
		public virtual int DoSomething(ref int data)
		{
			var result = data * 5;
			data *= 2;
			return result;
		}
	}
}