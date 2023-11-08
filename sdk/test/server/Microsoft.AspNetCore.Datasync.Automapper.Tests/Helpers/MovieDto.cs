// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;

namespace Microsoft.AspNetCore.Datasync.Automapper.Tests;

[ExcludeFromCodeCoverage]
public class MovieDto : ITableData, IMovie
{
    #region ITableData
    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool Deleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public byte[] Version { get; set; } = Array.Empty<byte>();
    #endregion

    #region IMovie
    /// <inheritdoc />
    public bool BestPictureWinner { get; set; }

    /// <inheritdoc />
    public int Duration { get; set; }

    /// <inheritdoc />
    public MovieRating Rating { get; set; }

    /// <inheritdoc />
    public DateOnly ReleaseDate { get; set; }

    /// <inheritdoc />
    public string Title { get; set; } = "";

    /// <inheritdoc />
    public int Year { get; set; }
    #endregion

    /// <inheritdoc />
    public bool Equals(ITableData other)
        => other != null && Id == other.Id && Version.SequenceEqual(other.Version);
}
