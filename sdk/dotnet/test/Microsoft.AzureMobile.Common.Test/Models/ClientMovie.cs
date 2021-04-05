// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AzureMobile.Common.Test.Models
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ClientMovie : ClientTableData, IMovie, IEquatable<IMovie>
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

        /// <summary>
        /// Determines if this movie has the same content as another movie.
        /// </summary>
        /// <param name="other">The other movie</param>
        /// <returns>true if the content is the same</returns>
        public bool Equals(IMovie other)
            => other != null
            && other.BestPictureWinner == BestPictureWinner
            && other.Duration == Duration
            && other.Rating == Rating
            && other.ReleaseDate == ReleaseDate
            && other.Title == Title
            && other.Year == Year;

        /// <summary>
        /// Clones this object into a new object.
        /// </summary>
        /// <returns></returns>
        public ClientMovie Clone()
            => new()
            {
                Id = this.Id,
                UpdatedAt = this.UpdatedAt,
                Version = this.Version,
                Deleted = this.Deleted,
                BestPictureWinner = this.BestPictureWinner,
                Duration = this.Duration,
                Rating = this.Rating,
                ReleaseDate = this.ReleaseDate,
                Title = this.Title,
                Year = this.Year
            };
    }
}
