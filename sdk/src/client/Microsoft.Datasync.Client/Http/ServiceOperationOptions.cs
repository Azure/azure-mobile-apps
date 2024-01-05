// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Http;

public struct ServiceOperationOptions
{
    /// <summary>
    /// If set, the operation will use conditional headers to ensure that the entity matches the provided version
    /// on the server side before performing the operation.
    /// </summary>
    public string? RequireVersion { get; set; }
}
