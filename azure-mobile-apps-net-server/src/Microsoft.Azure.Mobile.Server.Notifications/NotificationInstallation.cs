// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// The <see cref="NotificationInstallation"/> contains information for registering a device installation for
    /// push notifications through a notification hub.
    /// </summary>
    public class NotificationInstallation
    {
        private IDictionary<string, NotificationTemplate> templates;
        private IDictionary<string, NotificationSecondaryTile> secondaryTiles;
        private IList<string> tags;

        /// <summary>
        /// The channel URI if registering the installation for WNS; Device Token if registering for APNS.
        /// </summary>
        [Required]
        public string PushChannel { get; set; }

        /// <summary>
        /// The platform for this installation. The platform must be one of the following values: wns, apns.  
        /// Where wns stands for "Windows Push Notification Services", and apns is "Apple Push Notification Service".
        /// </summary>
        [Required]
        public string Platform { get; set; }

        /// <summary>
        /// The device installation id to register.
        /// </summary>
        public string InstallationId { get; set; }

        /// <summary>
        /// A dictionary of template names to <see cref="NotificationTemplate"/> objects.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set is used in Test.")]
        public IDictionary<string, NotificationTemplate> Templates
        {
            get
            {
                if (this.templates == null)
                {
                    this.templates = new Dictionary<string, NotificationTemplate>();
                }

                return this.templates;
            }

            set
            {
                this.templates = value;
            }
        }

        /// <summary>
        /// A dictionary of secondary tile names to <see cref="NotificationSecondaryTile"/> objects to register with this installation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set is used in Test.")]
        public IDictionary<string, NotificationSecondaryTile> SecondaryTiles
        {
            get
            {
                if (this.secondaryTiles == null)
                {
                    this.secondaryTiles = new Dictionary<string, NotificationSecondaryTile>();
                }

                return this.secondaryTiles;
            }

            set
            {
                this.secondaryTiles = value;
            }
        }

        /// <summary>
        /// A list of tags to register with this installation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Set is used in Test.")]
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
