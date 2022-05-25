// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Azure.Mobile.Server.Notifications;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// The <see cref="MpnsPushMessage"/> class helps in generating a notification payload targeting 
    /// Microsoft Push Notification Services. Notifications can be sent using the <see cref="PushClient"/>
    /// class.
    /// </summary>
    [XmlRoot("Notification", Namespace = MpnsPushMessage.XmlNamespace)]
    public class MpnsPushMessage : IPushMessage
    {
        internal const string XmlNamespace = "WPNotification";

        private static readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] 
        { 
            new XmlQualifiedName("wp", "WPNotification") 
        });

        private static readonly XmlWriterSettings SerializerSettings = new XmlWriterSettings()
        {
            Encoding = new UTF8Encoding(false),
            Indent = true
        };

        private IDictionary<string, string> headers;

        internal MpnsPushMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MpnsPushMessage"/> class with a given <paramref name="toastOrTile"/>.
        /// The toast or tile can be <see cref="CycleTile"/>, <see cref="FlipTile"/>, <see cref="IconicTile"/>,
        /// or <see cref="Toast"/> (defined in the <c>Notifications</c> namespace).
        /// </summary>
        /// <param name="toastOrTile">One of <see cref="CycleTile"/>, <see cref="FlipTile"/>, <see cref="IconicTile"/>, or <see cref="Toast"/>.</param>
        public MpnsPushMessage(MpnsMessage toastOrTile)
        {
            if (toastOrTile == null)
            {
                throw new ArgumentNullException("toastOrTile");
            }

            this.Message = toastOrTile;
        }

        /// <summary>
        /// Gets or sets the version of the of the toast or tile.
        /// </summary>
        [XmlAttribute]
        public string Version
        {
            get
            {
                return this.Message.Version;
            }

            set
            {
                this.Message.Version = value;
            }
        }

        /// <summary>
        /// Gets or sets the specific <see cref="MpnsMessage"/> for this instance.
        /// </summary>
        [XmlElement("Toast", typeof(Toast))]
        [XmlElement("Tile", typeof(MpnsTileMessage))]
        public MpnsMessage Message { get; set; }

        /// <summary>
        /// As an alternative to building the notification programmatically using a <see cref="CycleTile"/>, <see cref="FlipTile"/>
        /// <see cref="IconicTile"/>, or <see cref="Toast"/> (defined in the <c>Notifications</c> namespace), it is possible to provide a complete XML 
        /// representation which will be sent to the Notification Hub unaltered.
        /// </summary>
        public string XmlPayload { get; set; }

        /// <summary>
        /// Any additional HTTP headers sent to the Microsoft Push Notification Services along with the notification.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return this.headers;
            }
        }

        /// <summary>
        /// Provides an XML representation of the <see cref="MpnsPushMessage"/> instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // If raw value is set then use that; otherwise use the DOM
            if (this.XmlPayload != null)
            {
                return this.XmlPayload;
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlWriter xmlWriter = XmlWriter.Create(memStream, SerializerSettings);
                XmlSerializer serializer = new XmlSerializer(typeof(MpnsPushMessage));
                serializer.Serialize(xmlWriter, this, Namespaces);
                return Encoding.UTF8.GetString(memStream.ToArray());
            }
        }
    }
}
