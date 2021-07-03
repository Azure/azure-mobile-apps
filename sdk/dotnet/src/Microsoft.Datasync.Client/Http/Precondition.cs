// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A generic precondition header
    /// </summary>
    public class Precondition
    {
        internal Precondition(string headerName)
        {
            Validate.IsNotNullOrEmpty(headerName, nameof(headerName));
            HeaderName = headerName;
        }

        /// <summary>
        /// The name of the header
        /// </summary>
        public string HeaderName { get; }

        /// <summary>
        /// The value of the header
        /// </summary>
        public string HeaderValue { get; protected set; }

        /// <summary>
        /// A printable / loggable version of the object.
        /// </summary>
        public override string ToString() => $"PRECONDITION({HeaderName}={HeaderValue})";
    }
}
