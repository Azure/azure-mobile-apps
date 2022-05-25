// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Represents a Cycle Tile targeting MPNS. Use the <see cref="CycleTile"/> with 
    /// <see cref="MpnsPushMessage"/> to create an MPNS notification and send it using the 
    /// <see cref="PushClient"/>.
    /// </summary>
    /// <remarks>
    /// For more information about MPNS Cycle Tiles, see <c>http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj207036</c>.
    /// </remarks>
    public class CycleTile : MpnsTileMessage
    {
        private const string SmallBackgroundImageKey = "SmallBackgroundImage";
        private const string CycleImage1Key = "CycleImage1";
        private const string CycleImage2Key = "CycleImage2";
        private const string CycleImage3Key = "CycleImage3";
        private const string CycleImage4Key = "CycleImage4";
        private const string CycleImage5Key = "CycleImage5";
        private const string CycleImage6Key = "CycleImage6";
        private const string CycleImage7Key = "CycleImage7";
        private const string CycleImage8Key = "CycleImage8";
        private const string CycleImage9Key = "CycleImage9";

        /// <summary>
        /// Initializes a new instance of the <see cref="CycleTile"/> class.
        /// </summary>
        public CycleTile()
            : base(version: "2.0", template: "CycleTile")
        {
            this.Properties[SmallBackgroundImageKey] = null;
            this.Properties[CycleImage1Key] = null;
            this.Properties[CycleImage2Key] = null;
            this.Properties[CycleImage3Key] = null;
            this.Properties[CycleImage4Key] = null;
            this.Properties[CycleImage5Key] = null;
            this.Properties[CycleImage6Key] = null;
            this.Properties[CycleImage7Key] = null;
            this.Properties[CycleImage8Key] = null;
            this.Properties[CycleImage9Key] = null;
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
        /// Gets or sets background image 1 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage1
        {
            get
            {
                return this.Properties[CycleImage1Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage1Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 2 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage2
        {
            get
            {
                return this.Properties[CycleImage2Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage2Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 3 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage3
        {
            get
            {
                return this.Properties[CycleImage3Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage3Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 4 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage4
        {
            get
            {
                return this.Properties[CycleImage4Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage4Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 5 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage5
        {
            get
            {
                return this.Properties[CycleImage5Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage5Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 6 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage6
        {
            get
            {
                return this.Properties[CycleImage6Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage6Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 7 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage7
        {
            get
            {
                return this.Properties[CycleImage7Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage7Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 8 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage8
        {
            get
            {
                return this.Properties[CycleImage8Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage8Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets background image 9 for the medium and wide tile sizes.
        /// </summary>
        public Uri CycleImage9
        {
            get
            {
                return this.Properties[CycleImage9Key] as Uri;
            }

            set
            {
                this.Properties[CycleImage9Key] = value;
            }
        }
    }
}
