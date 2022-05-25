// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.Azure.Mobile.Server.Notifications;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// The <see cref="ApplePushMessage"/> helps generating a notification payload targeting 
    /// devices that have registered with a template registration. A template lets the device specify 
    /// the shape of notifications that it wants to receive including a set of keywords which it wants 
    /// the sender to fill in. Instead of the sender building the entire notification, it simply sets the
    /// keyword values. The Notification Hub will then build a notification using the particular template
    /// registered by device and the keywords provided by the sender. This makes it much easier to send
    /// notifications regardless of the platform of the receiver. The keywords defined by the 
    /// <see cref="TemplatePushMessage"/> class can be sent using the <see cref="PushClient"/> class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This describes a message.")]
    [Serializable]
    public class TemplatePushMessage : Dictionary<string, string>, IPushMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatePushMessage"/> class.
        /// </summary>
        public TemplatePushMessage()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatePushMessage"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="TemplatePushMessage"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected TemplatePushMessage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
