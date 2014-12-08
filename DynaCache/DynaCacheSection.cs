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
    /// The DynaCache configuration section.
    /// </summary>
    public class DynaCacheSection : ConfigurationSection
    {
        #region Public Properties

        /// <summary>
        /// Gets the set of configured cache durations.
        /// </summary>
        [ConfigurationProperty("cacheDurations", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(CacheDurationCollection), 
            AddItemName = "add", 
            ClearItemsName = "clear", 
            RemoveItemName = "remove")]
        public CacheDurationCollection CacheDurations
        {
            get
            {
                return (CacheDurationCollection)base["cacheDurations"];
            }
        }

        #endregion
    }
}
