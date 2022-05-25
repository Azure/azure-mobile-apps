// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Query
{
    internal class QueryToken
    {
        public int Position { get; set; }
        public string Text { get; set; }
        public QueryTokenKind Kind { get; set; }
    }
}
