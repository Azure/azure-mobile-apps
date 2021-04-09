﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AzureMobile.Common.Test.Models
{
    /// <summary>
    /// The base class for the Movie data.  This implements <see cref="IMovie"/>
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MovieBase : IMovie
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
        public string Title { get; set; }

        /// <summary>
        /// The year that the movie was released.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Copies the current data into another IMovie based object.
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(IMovie other)
        {
            other.BestPictureWinner = BestPictureWinner;
            other.Duration = Duration;
            other.Rating = Rating;
            other.ReleaseDate = ReleaseDate;
            other.Title = Title;
            other.Year = Year;
        }
    }
}