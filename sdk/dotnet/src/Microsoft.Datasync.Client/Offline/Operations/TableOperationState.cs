// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Offline.Operations
{
    /// <summary>
    /// Enumeration for the state of table operation.
    /// </summary>
    public enum TableOperationState
    {
        Pending = 0,
        Attempted,
        Failed
    }
}
