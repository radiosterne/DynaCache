#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.TestApp
{
	/// <summary>
	/// The test service interface.
	/// </summary>
	public interface IRandomService
	{
		/// <summary>
		/// Gets a random number between the given bounds.
		/// </summary>
		/// <param name="minInclusive">The minimum value to return (inclusive).</param>
		/// <param name="maxExclusive">The maximum value to return (exclusive).</param>
		/// <returns>The random number.</returns>
		int GetRandomNumber(int minInclusive, int maxExclusive);
	}
}
