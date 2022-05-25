// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// This class represents the <c>binding</c> element of a Windows Notification tile, see
    /// <c>http://msdn.microsoft.com/en-us/library/windows/apps/hh761491.aspx</c> for details.
    /// This class is intended for use as part of the <see cref="WindowsPushMessage"/> class.
    /// </summary>
    public class TileBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileBinding"/> class.
        /// </summary>
        public TileBinding()
            : this(new TileImage[0], new TileText[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileBinding"/> class.
        /// </summary>
        /// <param name="images">An initial set of <see cref="TileImage"/> for this tile.</param>
        public TileBinding(params TileImage[] images)
            : this(images, new TileText[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileBinding"/> class.
        /// </summary>
        /// <param name="texts">An initial set of <see cref="TileText"/> for this tile.</param>
        public TileBinding(params TileText[] texts)
            : this(new TileImage[0], texts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TileBinding"/> class.
        /// </summary>
        /// <param name="images">An initial set of <see cref="TileImage"/> for this tile.</param>
        /// <param name="texts">An initial set of <see cref="TileText"/> for this tile.</param>
        public TileBinding(IEnumerable<TileImage> images, IEnumerable<TileText> texts)
        {
            if (images == null)
            {
                throw new ArgumentNullException("images");
            }

            if (texts == null)
            {
                throw new ArgumentNullException("texts");
            }

            this.Images = new Collection<TileImage>(images.ToList());
            this.Texts = new Collection<TileText>(texts.ToList());
        }

        /// <summary>
        /// One of the provided templates on which to base the tile. Typically, a developer should supply both a square and 
        /// a wide format, each as a separate binding element. Valid entries are members of the tileTemplateType enumeration.
        /// </summary>
        [XmlAttribute("template")]
        public string Template { get; set; }

        /// <summary>
        /// A template to use if the primary template name is not recognized by the recipient, for use with Windows 8 compatibility. 
        /// This value is the Windows 8 name of the value in the template attribute. New templates introduced after Windows 8 do 
        /// not have a fallback.
        /// </summary>
        [XmlAttribute("fallback")]
        public string Fallback { get; set; }

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
        /// Set of <see cref="TileImage"/> elements
        /// </summary>
        [XmlElement("image")]
        public Collection<TileImage> Images { get; private set; }

        /// <summary>
        /// Set of <see cref="TileText"/> elements
        /// </summary>
        [XmlElement("text")]
        public Collection<TileText> Texts { get; private set; }
    }
}
