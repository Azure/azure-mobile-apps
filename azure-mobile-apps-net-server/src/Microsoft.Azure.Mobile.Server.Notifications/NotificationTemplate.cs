// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// A <see cref="NotificationTemplate"/> contains information for registering a template with a <see cref="NotificationInstallation"/>.
    /// </summary>
    public class NotificationTemplate
    {
        private IList<string> tags;

        /// <summary>
        /// The notification payload for this <see cref="NotificationTemplate"/>.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// For WNS, a dictionary of header names to values that specify the notification type (i.e. "X-WNS-TYPE":"wns/toast").
        /// For all other platforms this property is meaningless.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set is used in test.")]
        public IDictionary<string, string> Headers
        {
            get;
            set;
        }

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
    }
}
