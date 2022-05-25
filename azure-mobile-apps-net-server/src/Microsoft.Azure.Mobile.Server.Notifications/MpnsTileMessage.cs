// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Abstract base class for all MPNS tiles used by <see cref="MpnsPushMessage"/> and related classes. 
    /// </summary>
    /// <remarks>
    /// This class is not intended for direct use.
    /// </remarks>
    public abstract class MpnsTileMessage : MpnsMessage
    {
        private const string CountKey = "Count";
        private const string TitleKey = "Title";

        /// <summary>
        /// Internal constructor needed for serialization.
        /// </summary>
        internal MpnsTileMessage()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MpnsTileMessage"/> implementation with a given
        /// <paramref name="version"/> and <paramref name="template"/>.
        /// </summary>
        /// <param name="version">The version of the tile.</param>
        /// <param name="template">The name of the template.</param>
        protected MpnsTileMessage(string version, string template)
            : base(version, template)
        {
            this.Properties[CountKey] = null;
            this.Properties[TitleKey] = null;
        }

        /// <summary>
        /// Gets or sets the optional ID or the tile.
        /// </summary>
        public string Id
        {
            get
            {
                return this.TileId;
            }

            set
            {
                this.TileId = value;
            }
        }

        /// <summary>
        /// Gets or sets a value between 1 and 99 that will be displayed in the Count field of the Tile. A value of 0 
        /// means the Count will not be displayed. If this property is not set, the Count display will not change during 
        /// an update.
        /// </summary>
        public int? Count
        {
            get
            {
                return this.Properties[CountKey] as int?;
            }

            set
            {
                this.Properties[CountKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text that displays on the front side of the medium and wide tile sizes.
        /// </summary>
        public string Title
        {
            get
            {
                return this.Properties[TitleKey] as string;
            }

            set
            {
                this.Properties[TitleKey] = value;
            }
        }
    }
}
