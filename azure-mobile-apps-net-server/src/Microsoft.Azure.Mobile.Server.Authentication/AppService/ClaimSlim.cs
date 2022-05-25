//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Mobile.Server.Authentication.AppService
{
    using System.Runtime.Serialization;
    using System.Security.Claims;

    /// <summary>
    /// Light-weight representation of a <see cref="Claim"/> object.
    /// </summary>
    [DataContract]
    internal struct ClaimSlim
    {
        [DataMember(Name = "val")]
        internal string Value;

        [DataMember(Name = "typ")]
        internal string Type;

        internal ClaimSlim(string type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Gets a string showing the type and value of the claim. This is primarily intended for debugging.
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.Type, ": ", this.Value);
        }
    }
}
