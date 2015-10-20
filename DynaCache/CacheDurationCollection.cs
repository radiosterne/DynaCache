#region Copyright 2014 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
	#region Using declarations

	using System.Configuration;

	#endregion

	/// <summary>
	/// A collection of <see cref="CacheDuration"/> instances.
	/// </summary>
	public class CacheDurationCollection : ConfigurationElementCollection
	{
		/// <inheritdoc />
		public new CacheDuration this[string name]
		{
			get
			{
				var duration = BaseGet(name) as CacheDuration;
				if (duration == null)
				{
					throw new DynaCacheException("Unknown named cache referenced: " + name);
				}

				return duration;
			}
		}

		#region Methods

		/// <inheritdoc />
		protected override ConfigurationElement CreateNewElement()
		{
			return new CacheDuration();
		}

		/// <inheritdoc />
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((CacheDuration)element).Name;
		}

		#endregion
	}
}