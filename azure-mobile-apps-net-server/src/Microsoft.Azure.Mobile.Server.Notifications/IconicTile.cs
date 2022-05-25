// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Represents a Iconic Tile targeting MPNS. Use the <see cref="IconicTile"/> with 
    /// <see cref="IconicTile"/> to create an MPNS notification and send it using the 
    /// <see cref="PushClient"/>.
    /// </summary>
    /// <remarks>
    /// For more information about MPNS Iconic Tiles, see <c>http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj207009</c>.
    /// </remarks>
    public class IconicTile : MpnsTileMessage
    {
        private const string IconImageKey = "IconImage";
        private const string SmallIconImageKey = "SmallIconImage";
        private const string WideContent1Key = "WideContent1";
        private const string WideContent2Key = "WideContent2";
        private const string WideContent3Key = "WideContent3";
        private const string BackgroundColorKey = "BackgroundColor";

        /// <summary>
        /// Initialize a new instance of the <see cref="IconicTile"/> class.
        /// </summary>
        public IconicTile()
            : base(version: "2.0", template: "IconicTile")
        {
            this.Properties[IconImageKey] = null;
            this.Properties[SmallIconImageKey] = null;
            this.Properties[WideContent1Key] = null;
            this.Properties[WideContent2Key] = null;
            this.Properties[WideContent3Key] = null;
            this.Properties[BackgroundColorKey] = null;
        }

        /// <summary>
        /// Gets or sets the icon image for the medium and large tile sizes.
        /// </summary>
        public Uri IconImage
        {
            get
            {
                return this.Properties[IconImageKey] as Uri;
            }

            set
            {
                this.Properties[IconImageKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the icon image for the small tile size.
        /// </summary>
        public Uri SmallIconImage
        {
            get
            {
                return this.Properties[SmallIconImageKey] as Uri;
            }

            set
            {
                this.Properties[SmallIconImageKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text that displays on the first row of the wide tile size.
        /// </summary>
        public string WideContent1
        {
            get
            {
                return this.Properties[WideContent1Key] as string;
            }

            set
            {
                this.Properties[WideContent1Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text that displays on the second row of the wide tile size.
        /// </summary>
        public string WideContent2
        {
            get
            {
                return this.Properties[WideContent2Key] as string;
            }

            set
            {
                this.Properties[WideContent2Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text that displays on the third row of the wide tile size.
        /// </summary>
        public string WideContent3
        {
            get
            {
                return this.Properties[WideContent3Key] as string;
            }

            set
            {
                this.Properties[WideContent3Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color of the Tile. Setting this property overrides the 
        /// default theme color that is set on the phone.
        /// </summary>
        public string BackgroundColor
        {
            get
            {
                return this.Properties[BackgroundColorKey] as string;
            }

            set
            {
                this.Properties[BackgroundColorKey] = value;
            }
        }
    }
}
