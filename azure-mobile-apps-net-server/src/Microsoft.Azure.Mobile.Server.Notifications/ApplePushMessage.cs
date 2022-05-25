// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.Azure.Mobile.Server.Notifications;
using Microsoft.Azure.Mobile.Server.Properties;
using Newtonsoft.Json;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// The <see cref="ApplePushMessage"/> helps generating a notification payload targeting 
    /// Apple Push Notification Service. Notifications can be sent using the <see cref="PushClient"/>
    /// class.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "Expiration is not intended for serialization")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This describes a message.")]
    [Serializable]
    public class ApplePushMessage : Dictionary<string, object>, IPushMessage
    {
        private const string ApsKey = "aps";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplePushMessage"/> class enabling creation a notification
        /// message targeting Apple Push Notification Service. Set the appropriate properties on the message
        /// and submit through the <see cref="PushClient"/>.
        /// </summary>
        public ApplePushMessage()
            : base(StringComparer.OrdinalIgnoreCase)
        {
            this[ApsKey] = new ApsProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplePushMessage"/> class with a given <paramref name="alert"/>
        /// message and an optional <paramref name="expiration"/> of the notification. 
        /// </summary>
        /// <param name="alert">The notification alert message.</param>
        /// <param name="expiration">A <see cref="TimeSpan"/> relative to the current time. At the end of the lifetime, 
        /// the notification is no longer valid and can be discarded. If this value is non-null, APNs stores the 
        /// notification and tries to deliver the notification at least once. Specify null to indicate that the 
        /// notification expires immediately and that APNs should not store the notification at all.</param>
        public ApplePushMessage(string alert, TimeSpan? expiration)
            : this()
        {
            if (alert == null)
            {
                throw new ArgumentNullException("alert");
            }

            if (expiration.HasValue)
            {
                if (expiration.Value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("expiration", expiration.Value, RResources.ArgumentOutOfRange_GreaterThan.FormatForUser(TimeSpan.Zero));
                }

                this.Expiration = DateTimeOffset.UtcNow + expiration.Value;
            }

            this.Aps.Alert = alert;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplePushMessage"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="AlertProperties"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected ApplePushMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the <see cref="ApsProperties"/> for this <see cref="ApplePushMessage"/>.
        /// </summary>
        public ApsProperties Aps
        {
            get
            {
                return this.GetValueOrDefault<ApsProperties>(ApsKey);
            }
        }

        /// <summary>
        /// Sets or gets the lifetime of the notification. At the end of the lifetime, the notification is no 
        /// longer valid and can be discarded. If this value is non-null, APNs stores the notification and tries 
        /// to deliver the notification at least once. Specify null to indicate that the notification expires 
        /// immediately and that APNs should not store the notification at all.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// As an alternative to building the notification by initializing the <see cref="ApplePushMessage"/> directly, 
        /// it is possible to provide a complete JSON representation which will be sent to the Notification Hub unaltered.
        /// </summary>
        public string JsonPayload { get; set; }

        /// <summary>
        /// Provides a JSON encoded representation of this <see cref="ApplePushMessage"/>
        /// </summary>
        /// <returns>A JSON encoded string.</returns>
        public override string ToString()
        {
            // If raw value is set then use that; otherwise use the DOM
            if (this.JsonPayload != null)
            {
                return this.JsonPayload;
            }
            else
            {
                return JsonConvert.SerializeObject(this, SerializerSettings);
            }
        }
    }
}
