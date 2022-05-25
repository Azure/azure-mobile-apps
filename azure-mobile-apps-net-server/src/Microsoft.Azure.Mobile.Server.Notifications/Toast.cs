// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// Represents a Toast notification targeting MPNS. Use the <see cref="Toast"/> with 
    /// <see cref="MpnsPushMessage"/> to create an MPNS notification and send it using the 
    /// <see cref="PushClient"/>.
    /// </summary>
    /// <remarks>
    /// For more information about the MPNS Toast, see <c>http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj662938</c>.
    /// </remarks>
    public class Toast : MpnsMessage
    {
        private const string Text1Key = "Text1";
        private const string Text2Key = "Text2";
        private const string ParamKey = "Param";

        /// <summary>
        /// Initializes a new instance of the <see cref="Toast"/>.
        /// </summary>
        public Toast()
            : base(version: null, template: null)
        {
        }

        /// <summary>
        /// Gets or sets the <c>Text1</c> field of the toast.
        /// </summary>
        public string Text1
        {
            get
            {
                return this.Properties[Text1Key] as string;
            }

            set
            {
                this.Properties[Text1Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <c>Text2</c> field of the toast.
        /// </summary>
        public string Text2
        {
            get
            {
                return this.Properties[Text2Key] as string;
            }

            set
            {
                this.Properties[Text2Key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <c>Param</c> field of the toast.
        /// </summary>
        public string Parameter
        {
            get
            {
                return this.Properties[ParamKey] as string;
            }

            set
            {
                this.Properties[ParamKey] = value;
            }
        }
    }
}
