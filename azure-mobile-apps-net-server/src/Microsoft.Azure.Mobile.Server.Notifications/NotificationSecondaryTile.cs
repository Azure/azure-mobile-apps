// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// The <see cref="NotificationSecondaryTile"/> contains information for registering a WNS secondary tile with a <see cref="NotificationInstallation"/>.
    /// </summary>
    public class NotificationSecondaryTile
    {
        private IList<string> tags;

        /// <summary>
        /// The channel URI to register with the tile.
        /// </summary>
        public string PushChannel { get; set; }

        /// <summary>
        /// A set of tags to register with the tile.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set is used in test.")]
        public IList<string> Tags
        {
            get
            {
                if (this.tags == null)
                {
                    this.tags = new List<string>();
                }

                return this.tags;
            }

            set
            {
                this.tags = value;
            }
        }

        /// <summary>
        /// A dictionary of template names to <see cref="NotificationTemplate"/> objects to register with the tile.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set is used in test.")]
        public IDictionary<string, NotificationTemplate> Templates
        {
            get;
            set;
        }
    }
}
