// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// This class represents the <c>visual</c> element of a Windows Notification tile, see
    /// <c>http://msdn.microsoft.com/en-us/library/windows/apps/hh761491.aspx</c> for details.
    /// This class is intended for use as part of the <see cref="WindowsPushMessage"/> class.
    /// </summary>
    public class VisualTile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualTile"/> class.
        /// </summary>
        public VisualTile()
            : this(new TileBinding[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualTile"/> class.
        /// </summary>
        /// <param name="bindings">An initial set of bindings for this <see cref="VisualTile"/>.</param>
        public VisualTile(params TileBinding[] bindings)
        {
            if (bindings == null)
            {
                throw new ArgumentNullException("bindings");
            }

            this.Bindings = new Collection<TileBinding>(bindings.ToList());
            this.Version = 1;
        }

        /// <summary>
        /// The version of the tile XML schema this particular payload was developed for. It can have two 
        /// values, 1 or 2. Version 1 requires a valid payload under the Windows 8 schema. Version 2 
        /// recognizes the new large tile templates, the new Windows 8.1 template names for existing 
        /// templates, and the fallback attribute of the binding element.
        /// </summary>
        [DefaultValue(1)]
        [XmlAttribute("version")]
        public int Version { get; set; }

        /// <summary>
        /// The target locale of the XML payload, specified as a BCP-47 language tags such as <c>en-US</c> or <c>fr-FR</c>. 
        /// The locale specified here overrides any other specified locale, such as that in binding or visual. 
        /// If this value is a literal string, this attribute defaults to the user's UI language. If this value is a 
        /// string reference, this attribute defaults to the locale chosen by Windows Runtime in resolving the string.
        /// </summary>
        [XmlAttribute("lang")]
        public string Lang { get; set; }

        /// <summary>
        /// A default base URI that is combined with relative URIs in image source attributes.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "XmlSerializer doesn't handle System.Uri")]
        [XmlAttribute("baseUri")]
        public string BaseUri { get; set; }

        /// <summary>
        /// The form that the tile should use to display the app's brand.
        /// </summary>
        [XmlAttribute("branding")]
        public string Branding { get; set; }

        /// <summary>
        /// Set to true to allow Windows to append a query string to the image URI supplied in the tile notification. Use this attribute 
        /// if your server hosts images and can handle query strings, either by retrieving an image variant based on the query strings 
        /// or by ignoring the query string and returning the image as specified without the query string. This query string specifies 
        /// scale, contrast setting, and language; for instance, a value of <c>www.website.com/images/hello.png</c> included in the 
        /// notification becomes <c>www.website.com/images/hello.png?ms-scale=100&amp;ms-contrast=standard&amp;ms-lang=en-us</c>.
        /// </summary>
        [DefaultValueAttribute(false)]
        [XmlAttribute("addImageQuery")]
        public bool AddImageQuery { get; set; }

        /// <summary>
        /// Set to a sender-defined string that uniquely identifies the content of the notification. This prevents duplicates in the 
        /// situation where a large tile template is displaying the last three wide tile notifications. 
        /// </summary>
        [XmlAttribute("contentId")]
        public string ContentId { get; set; }

        /// <summary>
        /// The <see cref="TileBinding"/> specifies a tile templates. Every notification should include one binding element for each supported tile size.
        /// </summary>
        [XmlElement("binding")]
        public Collection<TileBinding> Bindings { get; private set; }
    }
}
