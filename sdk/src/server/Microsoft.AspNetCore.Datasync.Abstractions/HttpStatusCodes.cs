// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.Abstractions;

/// <summary>
/// A set of HTTP Status Codes.  We use this to avoid a dependency on Microsoft.AspNetCore.Http.
/// </summary>
public static class HttpStatusCodes
{
    public const int Status400BadRequest = 400;
    public const int Status404NotFound = 404;
    public const int Status409Conflict = 409;
    public const int Status412PreconditionFailed = 412;
}
