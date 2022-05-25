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
    /// The <see cref="GooglePushMessage"/> helps generating a notification payload targeting 
    /// Google Cloud Messaging for Chrome (GCM). Notifications can be sent using the <see cref="PushClient"/>
    /// class.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "Expiration is not intended for serialization")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This describes a message.")]
    [Serializable]
    public class GooglePushMessage : Dictionary<string, object>, IPushMessage
    {
        private const string CollapseKeyKey = "collapse_key";
        private const string DelayWhileIdleKey = "delay_while_idle";
        private const string DataKey = "data";
        private const string TimeToLiveKey = "time_to_live";

        private static readonly TimeSpan MaxExpiration = TimeSpan.FromDays(28);

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="GooglePushMessage"/> class enabling creation
        /// of a notification message targeting Google Cloud Messaging for Chrome (GCM).Set the 
        /// appropriate properties on the message and submit through the <see cref="PushClient"/>
        /// </summary>
        public GooglePushMessage()
            : base(StringComparer.OrdinalIgnoreCase)
        {
            this[DataKey] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GooglePushMessage"/> class with a given
        /// set of <paramref name="data"/> parameters and an optional <paramref name="timeToLive"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="timeToLive">A <see cref="TimeSpan"/> relative to the current time. The value of this 
        /// parameter must be a duration from 0 to 2,419,200 seconds (28 days), and it corresponds to the maximum period of time 
        /// for which GCM will store and try to deliver the message. Requests that don't contain this field default 
        /// to the maximum period of 4 weeks.</param>
        public GooglePushMessage(IDictionary<string, string> data, TimeSpan? timeToLive)
            : this()
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (timeToLive.HasValue)
            {
                if (timeToLive.Value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("timeToLive", timeToLive.Value, RResources.ArgumentOutOfRange_GreaterThan.FormatForUser(TimeSpan.Zero));
                }

                if (timeToLive.Value > MaxExpiration)
                {
                    throw new ArgumentOutOfRangeException("timeToLive", timeToLive.Value, RResources.ArgumentOutOfRange_LessThan.FormatForUser(MaxExpiration));
                }

                this.TimeToLiveInSeconds = (int)timeToLive.Value.TotalSeconds;
            }

            foreach (KeyValuePair<string, string> value in data)
            {
                this.Data.Add(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GooglePushMessage"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="GooglePushMessage"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected GooglePushMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this[DataKey] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// A collapse key is an arbitrary string that is used to collapse a group of like messages when the device 
        /// is offline, so that only the most recent message gets sent to the client. For example, "New mail", "Updates available", 
        /// and so on.
        /// </summary>
        public string CollapseKey
        {
            get
            {
                return this.GetValueOrDefault<string>(CollapseKeyKey);
            }

            set
            {
                this.SetOrClearValue(CollapseKeyKey, value);
            }
        }

        /// <summary>
        /// Indicates whether the message should be delivered while the device is idle.
        /// </summary>
        public bool DelayWhileIdle
        {
            get
            {
                return this.GetValueOrDefault<bool>(DelayWhileIdleKey);
            }

            set
            {
                this.SetOrClearValue(DelayWhileIdleKey, value);
            }
        }

        /// <summary>
        /// A collection or name-value properties to include in the message. Properties must be simple types, i.e. they 
        /// can not be nested.
        /// </summary>
        public IDictionary<string, string> Data
        {
            get
            {
                return this.GetValueOrDefault<IDictionary<string, string>>(DataKey);
            }
        }

        /// <summary>
        /// The Time to Live (TTL) property lets the sender specify the maximum lifespan of a message. The value of this 
        /// parameter must be a duration from 0 to 2,419,200 seconds, and it corresponds to the maximum period of time 
        /// for which GCM will store and try to deliver the message. Requests that don't contain this field default 
        /// to the maximum period of 4 weeks.
        /// </summary>
        public int? TimeToLiveInSeconds
        {
            get
            {
                return this.GetValueOrDefault<int?>(TimeToLiveKey);
            }

            set
            {
                this.SetOrClearValue(TimeToLiveKey, value);
            }
        }

        /// <summary>
        /// As an alternative to building the notification by initializing the <see cref="GooglePushMessage"/> directly, 
        /// it is possible to provide a complete JSON representation which will be sent to the Notification Hub unaltered.
        /// </summary>
        public string JsonPayload { get; set; }

        /// <summary>
        /// Provides a JSON encoded representation of this <see cref="GooglePushMessage"/>
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
