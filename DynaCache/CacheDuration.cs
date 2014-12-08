#region Copyright 2014 Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

namespace DynaCache
{
    #region Using declarations

    using System;
    using System.Configuration;

    #endregion

    /// <summary>
    /// Information about a configured cache duration.
    /// </summary>
    public class CacheDuration : ConfigurationElement
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the length of time that data associated to this cache duration should be cached for.
        /// </summary>
        [ConfigurationProperty("duration")]
        public TimeSpan Duration
        {
            get { return (TimeSpan)this["duration"]; }
            set { this["duration"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the cache duration. This can be referred to in <see cref="CacheableMethodAttribute"/>s.
        /// </summary>
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        #endregion
    }
}