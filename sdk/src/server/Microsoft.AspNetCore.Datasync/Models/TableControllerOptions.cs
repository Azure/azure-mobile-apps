// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// A list of options for configuring the <see cref="TableController{TEntity}"/>.
/// </summary>
public class TableControllerOptions
{
    /// <summary>
    /// The maximum page size that can be specified by the server.
    /// </summary>
    public const int MAX_PAGESIZE = 128000;

    /// <summary>
    /// The maximum value of $top that the client can request.
    /// </summary>
    public const int MAX_TOP = 128000;

    private int _pageSize = 100;
    private int _maxTop = MAX_TOP;

    /// <summary>
    /// The default page size for the results returned by a query operation.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set { Ensure.That(value).IsInRange(1, MAX_PAGESIZE); _pageSize = value; }
    }

    /// <summary>
    /// The maximum page size for the results returned by a query operation.  This is the
    /// maximum value that the client can specify for the <c>$top</c> query option.
    /// </summary>
    public int MaxTop
    {
        get => _maxTop;
        set { Ensure.That(value).IsInRange(1, MAX_TOP); _maxTop = value; }
    }

    /// <summary>
    /// If <c>true</c>, then items are marked as deleted instead of being removed from the database.
    /// By default, soft delete is turned off.
    /// </summary>
    public bool EnableSoftDelete { get; set; } = false;

    /// <summary>
    /// The status code returned when the user is not authorized to perform an operation.
    /// </summary>
    public int UnauthorizedStatusCode { get; set; } = StatusCodes.Status401Unauthorized;
}
