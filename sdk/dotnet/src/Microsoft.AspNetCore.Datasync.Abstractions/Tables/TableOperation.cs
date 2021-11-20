// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// The valid operations passed to an <see cref="IAccessControlProvider{TEntity}"/> method
    /// to show what the client is requesting.
    /// </summary>
    public enum TableOperation
    {
        Create,
        Delete,
        Query,
        Read,
        Update
    }
}
