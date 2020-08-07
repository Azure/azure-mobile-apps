// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// A list of operations that you can use to call the table controller.
    /// This is used when validating the authorization of the request in
    /// <see cref="TableControllerOptions.IsAuthorized()"/>.
    /// </summary>
    public enum TableOperation
    {
        None,
        Create,
        Delete,
        List,
        Patch,
        Read,
        Replace
    }
}
