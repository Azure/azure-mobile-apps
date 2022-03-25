// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// The list of OData query parameters used to construct an OData query string.
    /// </summary>
    internal static class ODataOptions
    {
        public const string Filter = "$filter";
        public const string OrderBy = "$orderby";
        public const string Skip = "$skip";
        public const string Top = "$top";
        public const string Select = "$select";
        public const string InlineCount = "$count";

        /// <summary>
        /// The query parameter used to include deleted items.  This is an OData
        /// extension for the Datasync service.
        /// </summary>
        public const string IncludeDeleted = "__includedeleted";
    }
}
