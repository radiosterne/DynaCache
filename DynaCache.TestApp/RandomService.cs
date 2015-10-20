#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache.TestApp
{
	using System;

	/// <summary>
	/// The test service.
	/// </summary>
	public class RandomService : IRandomService
	{
		/// <summary>
		/// The random instance to use when generating random numbers.
		/// </summary>
		private readonly Random _random = new Random();

		/// <summary>
		/// Gets a random number between the given bounds.
		/// </summary>
		/// <param name="minInclusive">The minimum value to return (inclusive).</param>
		/// <param name="maxExclusive">The maximum value to return (exclusive).</param>
		/// <returns>
		/// The random number.
		/// </returns>
		[CacheableMethod(1)]
		public virtual int GetRandomNumber(int minInclusive, int maxExclusive)
		{
			return _random.Next(minInclusive, maxExclusive);
		}
	}
}
