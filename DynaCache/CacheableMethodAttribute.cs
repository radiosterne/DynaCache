#region Copyright 2012 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
	using System;
	using System.Configuration;

	/// <summary>
	/// An attribute that can be applied to a method, causing the results of the method to be cached, varying by the parameter
	/// values being passed into it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CacheableMethodAttribute : Attribute
	{
		/// <summary>
		/// There's a better way to do this, i.e. configuration reading needs to take place outside the attribute; 
		/// in Azure, for example, this should be CloudConfigurationManager. For now, we aren't supporting that, so this will suffice.
		/// </summary>
		private static readonly DynaCacheSection Configuration = ConfigurationManager.GetSection("dynaCache") as DynaCacheSection;

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheableMethodAttribute"/> class.
		/// </summary>
		/// <param name="namedCacheDuration">The name of the cache duration to look up in the configuration file.</param>
		public CacheableMethodAttribute(string namedCacheDuration)
		{
			CacheSeconds = (int)Configuration.CacheDurations[namedCacheDuration].Duration.TotalSeconds;
		}

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
		public int CacheSeconds
		{
			get;
			private set;
		}
	}
}
