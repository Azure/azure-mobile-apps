
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A list of HTTP headers that the datasync service may expect.
    /// </summary>
    internal static class ServiceHeaders
    {
        /// <summary>
        /// The conditional "If-Match" header.
        /// </summary>
        internal const string IfMatch = "If-Match";

        /// <summary>
        /// The unique ID of the installation.
        /// </summary>
        internal const string InstallationId = "X-ZUMO-INSTALLATION-ID";

        /// <summary>
        /// Some clients can't adjust <c>User-Agent</c>, so we provide a secondary user-agent
        /// </summary>
        internal const string InternalUserAgent = "X-ZUMO-VERSION";

        /// <summary>
        /// The version of the protocol being used.
        /// </summary>
        internal const string ProtocolVersion = "ZUMO-API-VERSION";

        /// <summary>
        /// The <c>User-Agent</c> header name (so we don't need to bring in a NuGet for it)
        /// </summary>
        internal const string UserAgent = "User-Agent";
    }
}
