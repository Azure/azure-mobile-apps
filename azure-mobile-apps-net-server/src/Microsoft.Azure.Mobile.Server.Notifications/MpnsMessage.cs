// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Common base class used by <see cref="MpnsPushMessage"/> and related classes. 
    /// </summary>
    /// <remarks>
    /// This class is not intended for direct use; it is used by <see cref="FlipTile"/>, <see cref="CycleTile"/>,
    /// and <see cref="IconicTile"/>.
    /// </remarks>
    public abstract class MpnsMessage : IXmlSerializable
    {
        private readonly IDictionary<string, object> properties = new Dictionary<string, object>();

        internal MpnsMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MpnsMessage"/> class with a given <paramref name="version"/>
        /// and <paramref name="template"/>
        /// </summary>
        /// <param name="version">Optional version of the tile, e.g. "2.0", or null.</param>
        /// <param name="template">Optional name of the template, e.g. "FlipTile", or null.</param>
        protected MpnsMessage(string version, string template)
        {
            this.Version = version;
            this.Template = template;
        }

        /// <summary>
        /// Gets or sets the version of the tile or toast, e.g. "2.0", or <c>null</c> if there is no version.
        /// </summary>
        protected internal string Version { get; set; }

        /// <summary>
        /// Gets or sets the template name of the tile or toast, e.g. "FlipTile", or <c>null</c> if there is no template name.
        /// </summary>
        protected string Template { get; set; }

        /// <summary>
        /// Gets or sets the id of the tile. 
        /// </summary>
        protected string TileId { get; set; }

        /// <summary>
        /// Gets the properties associated with this toast or tile.
        /// </summary>
        protected IDictionary<string, object> Properties
        {
            get
            {
                return this.properties;
            }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (!string.IsNullOrEmpty(this.TileId))
            {
                writer.WriteAttributeString("Id", this.TileId);
            }

            if (!string.IsNullOrEmpty(this.Template))
            {
                writer.WriteAttributeString("Template", this.Template);
            }

            foreach (KeyValuePair<string, object> keyValuePair in this.properties)
            {
                string key = XmlConvert.EncodeLocalName(keyValuePair.Key);
                object value = keyValuePair.Value;
                writer.WriteStartElement(key, MpnsPushMessage.XmlNamespace);
                if (value != null)
                {
                    Uri address = value as Uri;
                    if (address != null && !address.IsAbsoluteUri)
                    {
                        writer.WriteAttributeString("IsRelative", "true");
                    }

                    writer.WriteValue(value);
                }
                else
                {
                    writer.WriteAttributeString("Action", "Clear");
                }

                writer.WriteEndElement();
            }
        }
    }
}
