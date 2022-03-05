// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// A single token returned by the OData expression tokenizer.
    /// </summary>
    internal class QueryToken
    {
        /// <summary>
        /// The type of token.
        /// </summary>
        public QueryTokenKind Kind { get; set; }

        /// <summary>
        /// The position of this token within the expression.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The text of the token.
        /// </summary>
        public string Text { get; set; }
    }
}
