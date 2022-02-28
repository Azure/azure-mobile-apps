// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Query.Linq.Nodes
{
    /// <summary>
    /// Enumeration for the different kinds of QueryNode.
    /// </summary>
    public enum QueryNodeKind
    {
        Constant = 0,
        UnaryOperator,
        BinaryOperator,
        FunctionCall,
        MemberAccess,
        Convert
    }
}
