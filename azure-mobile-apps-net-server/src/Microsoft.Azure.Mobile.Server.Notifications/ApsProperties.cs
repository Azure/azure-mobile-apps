// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// The "aps" property contains the definition of a notification targeting Apple Push Notification Service (APNS). It is intended
    /// to be used from the <see cref="ApplePushMessage"/> class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This is a property bag.")]
    [Serializable]
    public class ApsProperties : Dictionary<string, object>
    {
        private const string AlertKey = "alert";
        private const string BadgeKey = "badge";
        private const string SoundKey = "sound";
        private const string ContentAvailableKey = "content-available";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApsProperties"/> class.
        /// </summary>
        public ApsProperties()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApsProperties"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="ApsProperties"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected ApsProperties(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// The alert message as a single string. For more complex alert message options, please use <see cref="M:AlertProperties"/>.
        /// </summary>
        public string Alert
        {
            get
            {
                return this.GetValueOrDefault<string>(AlertKey);
            }

            set
            {
                this.SetOrClearValue(AlertKey, value);
            }
        }

        /// <summary>
        /// The alert message as a dictionary with additional properties describing the alert such as localization information, which image to display, etc.
        /// If the alert is simply a string then please use <see cref="M:Alert"/>.
        /// </summary>
        public AlertProperties AlertProperties
        {
            get
            {
                AlertProperties alerts = this.GetValueOrDefault<AlertProperties>(AlertKey);
                if (alerts == null)
                {
                    alerts = new AlertProperties();
                    this[AlertKey] = alerts;
                }

                return alerts;
            }
        }

        /// <summary>
        /// The number to display as the badge of the application icon. If this property is absent, the badge is not changed. 
        /// To remove the badge, set the value of this property to 0.
        /// </summary>
        public int? Badge
        {
            get
            {
                return this.GetValueOrDefault<int?>(BadgeKey);
            }

            set
            {
                this.SetOrClearValue(BadgeKey, value);
            }
        }

        /// <summary>
        /// The name of a sound file in the application bundle. The sound in this file is played as an alert. If the sound file 
        /// doesn’t exist or default is specified as the value, the default alert sound is played. The audio must be in one 
        /// of the audio data formats that are compatible with system sounds; 
        /// </summary>
        public string Sound
        {
            get
            {
                return this.GetValueOrDefault<string>(SoundKey);
            }

            set
            {
                this.SetOrClearValue(SoundKey, value);
            }
        }

        /// <summary>
        /// Provide this key with a value of 1 to indicate that new content is available. This is used to support 
        /// Newsstand apps and background content downloads.
        /// </summary>
        public bool ContentAvailable
        {
            get
            {
                return this.GetValueOrDefault<bool>(ContentAvailableKey);
            }

            set
            {
                this.SetOrClearValue(ContentAvailableKey, value);
            }
        }
    }
}
