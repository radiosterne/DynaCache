#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using DynaCache.Attributes;

namespace DynaCache.TestApp
{
	using System;

	/// <summary>
	/// The test service.
	/// </summary>
	public class RandomService : IRandomService
	{
		private readonly Random _random;

		public RandomService(Random random)
		{
			_random = random;
		}

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
