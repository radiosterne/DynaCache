#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System;

namespace DynaCache.Attributes
{
	/// <summary>
	/// An attribute that can be applied to a method, causing the results of the method to be cached, varying by the parameter
	/// values being passed into it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CacheableMethodAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CacheableMethodAttribute"/> class.
		/// </summary>
		/// <param name="duration">The duration for which to cache the result of the method, in seconds.</param>
		public CacheableMethodAttribute(int duration)
		{
			CacheSeconds = duration;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheableMethodAttribute"/> class.
		/// </summary>
		/// <param name="duration">The duration for which to cache the result of the method.</param>
		public CacheableMethodAttribute(TimeSpan duration)
		{
			CacheSeconds = (int)duration.TotalSeconds;
		}

		/// <summary>
		/// Gets the number of seconds to cache the results for.
		/// </summary>
		public int CacheSeconds { get; }
	}
}
