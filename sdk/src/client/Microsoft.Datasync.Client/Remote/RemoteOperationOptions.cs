// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Remote;

/// <summary>
/// The options that can be defined for a remote operation.
/// </summary>
public class RemoteOperationOptions
{
    /// <summary>
    /// If <c>false</c>, the operation will only be allowed if the version of the entity
    /// on the server matches the version of the entity being sent.  If <c>true</c>, the
    /// version on the server is ignored.
    /// </summary>
    public bool Force { get; set; }
}
