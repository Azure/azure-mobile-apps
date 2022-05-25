// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// This class represents the <c>image</c> element of a Windows Notification tile, see
    /// <c>http://msdn.microsoft.com/en-us/library/windows/apps/hh761491.aspx</c> for details.
    /// This class is intended for use as part of the <see cref="WindowsPushMessage"/> class.
    /// </summary>
    public class TileImage
    {
        /// <summary>
        /// The image element in the tile template that this image is intended for. 
        /// If a template has only one image, then this value is 1. The number of available 
        /// image positions is based on the template definition.
        /// </summary>
        [XmlAttribute("id")]
        public int Id { get; set; }

        /// <summary>
        /// The URI of the image source, one of these protocol handlers: <c>http://</c> or <c>https://</c> means a web-based image,
        /// <c>ms-appx:///</c> means an image included in the app package, <c>ms-appdata:///local/</c> means an image saved to local storage,
        /// and <c>file:///</c> means a local image. (Supported only for desktop apps. This protocol cannot be used by Windows Store apps.)
        /// </summary>
        [XmlAttribute("src")]
        public string Src { get; set; }

        /// <summary>
        /// A description of the image, for users of assistive technologies.
        /// </summary>
        [XmlAttribute("alt")]
        public string Alt { get; set; }

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
    }
}
