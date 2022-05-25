// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// This class represents the <c>text</c> element of a Windows Notification tile, see
    /// <c>http://msdn.microsoft.com/en-us/library/windows/apps/hh761491.aspx</c> for details.
    /// This class is intended for use as part of the <see cref="WindowsPushMessage"/> class.
    /// </summary>
    public class TileText
    {
        /// <summary>
        /// The text element in the tile template that this text is intended for. If a template has only one text 
        /// element, then this value is 1. The number of available text positions is based on the template definition.
        /// </summary>
        [XmlAttribute("id")]
        public int Id { get; set; }

        /// <summary>
        /// The target locale of the XML payload, specified as a BCP-47 language tags such as <c>en-US</c> or <c>fr-FR</c>. 
        /// The locale specified here overrides any other specified locale, such as that in binding or visual. 
        /// If this value is a literal string, this attribute defaults to the user's UI language. If this value is a 
        /// string reference, this attribute defaults to the locale chosen by Windows Runtime in resolving the string.
        /// </summary>
        [XmlAttribute("lang")]
        public string Lang { get; set; }

        /// <summary>
        /// The actual text of the tile element.
        /// </summary>
        [XmlText]
        public string Text { get; set; }
    }
}
