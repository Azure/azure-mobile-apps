// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Represents a Flip Tile targeting MPNS. Use the <see cref="FlipTile"/> with 
    /// <see cref="MpnsPushMessage"/> to create an MPNS notification and send it using the 
    /// <see cref="PushClient"/>.
    /// </summary>
    /// <remarks>
    /// For more information about MPNS Flip Tiles, see <c>http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206971</c>.
    /// </remarks>
    public class FlipTile : MpnsTileMessage
    {
        private const string SmallBackgroundImageKey = "SmallBackgroundImage";
        private const string WideBackgroundImageKey = "WideBackgroundImage";
        private const string WideBackBackgroundImageKey = "WideBackBackgroundImage";
        private const string WideBackContentKey = "WideBackContent";
        private const string BackgroundImageKey = "BackgroundImage";
        private const string BackBackgroundImageKey = "BackBackgroundImage";
        private const string BackTitleKey = "BackTitle";
        private const string BackContentKey = "BackContent";

        /// <summary>
        /// Initialize a new instance of the <see cref="FlipTile"/> class.
        /// </summary>
        public FlipTile()
            : base(version: "2.0", template: "FlipTile")
        {
            this.Properties[SmallBackgroundImageKey] = null;
            this.Properties[WideBackgroundImageKey] = null;
            this.Properties[WideBackBackgroundImageKey] = null;
            this.Properties[WideBackContentKey] = null;
            this.Properties[BackgroundImageKey] = null;
            this.Properties[BackBackgroundImageKey] = null;
            this.Properties[BackTitleKey] = null;
            this.Properties[BackContentKey] = null;
        }

        /// <summary>
        /// Gets and sets the front-side background image for the small tile size.
        /// </summary>
        public Uri SmallBackgroundImage
        {
            get
            {
                return this.Properties[SmallBackgroundImageKey] as Uri;
            }

            set
            {
                this.Properties[SmallBackgroundImageKey] = value;
            }
        }

        /// <summary>
        /// Gets and sets the front-side background image for the wide tile size.
        /// </summary>
        public Uri WideBackgroundImage
        {
            get
            {
                return this.Properties[WideBackgroundImageKey] as Uri;
            }

            set
            {
                this.Properties[WideBackgroundImageKey] = value;
            }
        }

        /// <summary>
        /// Gets and sets the back-side background image for the wide tile size.
        /// </summary>
        public Uri WideBackBackgroundImage
        {
            get
            {
                return this.Properties[WideBackBackgroundImageKey] as Uri;
            }

            set
            {
                this.Properties[WideBackBackgroundImageKey] = value;
            }
        }

        /// <summary>
        /// Gets and sets the text that displays above the title, on the back-side of the wide tile size. 
        /// </summary>
        public string WideBackContent
        {
            get
            {
                return this.Properties[WideBackContentKey] as string;
            }

            set
            {
                this.Properties[WideBackContentKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the background image of the front of the tile. If this property is an empty URI, the background image 
        /// of the front of the tile will not change during an update. 
        /// </summary>
        public Uri BackgroundImage
        {
            get
            {
                return this.Properties[BackgroundImageKey] as Uri;
            }

            set
            {
                this.Properties[BackgroundImageKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets a background image of the back of the tile. If this property is an empty URI, 
        /// the background image of the back of the tile will not change during an update. 
        /// </summary>
        public Uri BackBackgroundImage
        {
            get
            {
                return this.Properties[BackBackgroundImageKey] as Uri;
            }

            set
            {
                this.Properties[BackBackgroundImageKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the title to display at the bottom of the back of the tile. If this property is an empty string, 
        /// the title on the back of the tile will not change during an update.
        /// </summary>
        public string BackTitle
        {
            get
            {
                return this.Properties[BackTitleKey] as string;
            }

            set
            {
                this.Properties[BackTitleKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to display on the back of the tile, above the title. If this 
        /// property is an empty string, the content on the back of the tile will not change
        /// during an update.
        /// </summary>
        public string BackContent
        {
            get
            {
                return this.Properties[BackContentKey] as string;
            }

            set
            {
                this.Properties[BackContentKey] = value;
            }
        }
    }
}
