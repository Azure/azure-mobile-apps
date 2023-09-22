// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Datasync.Common.Test.Models;

/// <summary>
/// An abstraction of the movie data used for query and read tests. This
/// data is provided because it is one of each type that we support.
/// </summary>
public interface IMovie
{
    /// <summary>
    /// True if the movie won the oscar for Best Picture
    /// </summary>
    bool BestPictureWinner { get; set; }

    /// <summary>
    /// The running time of the movie
    /// </summary>
    int Duration { get; set; }

    /// <summary>
    /// The MPAA rating for the movie, if available.
    /// </summary>
    string Rating { get; set; }

    /// <summary>
    /// The release date of the movie.
    /// </summary>
    DateTimeOffset ReleaseDate { get; set; }

    /// <summary>
    /// The title of the movie.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// The year that the movie was released.
    /// </summary>
    int Year { get; set; }
}
