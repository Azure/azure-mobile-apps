// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;

namespace Microsoft.AspNetCore.Datasync.Automapper.Test.Helpers;

[ExcludeFromCodeCoverage]
public class MovieDto : EntityTableData, IMovie
{
    /// <summary>
    /// True if the movie won the oscar for Best Picture
    /// </summary>
    public bool BestPictureWinner { get; set; }

    /// <summary>
    /// The running time of the movie
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// The MPAA rating for the movie, if available.
    /// </summary>
    public string Rating { get; set; }

    /// <summary>
    /// The release date of the movie.
    /// </summary>
    public DateTimeOffset ReleaseDate { get; set; }

    /// <summary>
    /// The title of the movie.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// The year that the movie was released.
    /// </summary>
    public int Year { get; set; }
}
