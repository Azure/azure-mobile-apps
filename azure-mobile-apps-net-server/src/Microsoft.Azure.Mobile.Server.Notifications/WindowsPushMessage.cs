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
    /// The <see cref="WindowsPushMessage"/> helps generating a notification payload targeting 
    /// Windows Push Notification Services. Notifications can be sent using the <see cref="PushClient"/>
    /// class.
    /// </summary>
    [XmlRoot("tile")]
    public class WindowsPushMessage : IPushMessage
    {
        private static readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName() });
        private static readonly XmlWriterSettings SerializerSettings = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true,
            Encoding = new UTF8Encoding(false),
            Indent = true
        };

        private IDictionary<string, string> headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPushMessage"/> class.
        /// </summary>
        public WindowsPushMessage()
            : this(1, new TileBinding[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPushMessage"/> class.
        /// </summary>
        /// <param name="version">The version of the tile XML schema this particular payload was developed for (e.g. "1" or "2").</param>
        /// <param name="bindings">An initial set of bindings for this <see cref="WindowsPushMessage"/>.</param>
        public WindowsPushMessage(int version, params TileBinding[] bindings)
        {
            if (bindings == null)
            {
                throw new ArgumentNullException("bindings");
            }

            this.Visual = new VisualTile(bindings)
            {
                Version = version
            };
        }

        /// <summary>
        /// As an alternative to building the notification programmatically by adding <see cref="TileBinding"/> instances to the
        /// <see cref="WindowsPushMessage"/>, it is possible to provide a complete XML representation which will be sent 
        /// to the Notification Hub unaltered.
        /// </summary>
        public string XmlPayload { get; set; }

        /// <summary>
        /// Any additional HTTP headers sent to the Windows Push Notification Services along with the notification.
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
        /// A <see cref="VisualTile"/> element which contains multiple binding child elements, each of which defines a tile.
        /// </summary>
        [XmlElement("visual")]
        public VisualTile Visual { get; set; }

        /// <summary>
        /// Provides an XML representation of the <see cref="WindowsPushMessage"/> instance.
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
                XmlSerializer serializer = new XmlSerializer(typeof(WindowsPushMessage));
                serializer.Serialize(xmlWriter, this, Namespaces);
                return Encoding.UTF8.GetString(memStream.ToArray());
            }
        }
    }
}
