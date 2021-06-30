// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Microsoft.Datasync.Client.Integration.Helpers
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Movie : DatasyncClientData, IEquatable<Movie>
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
        public bool Equals(Movie other)
            => other != null
            && other.Id == Id
            && other.BestPictureWinner == BestPictureWinner
            && other.Duration == Duration
            && other.Rating == Rating
            && other.ReleaseDate == ReleaseDate
            && other.Title == Title
            && other.Year == Year;

        /// <summary>
        /// Override of the default <see cref="Equals(object)"/> method, since that is
        /// recommended by Intellicode/Roslynator.
        /// </summary>
        /// <param name="obj">The comparison object</param>
        /// <returns>true if the provided object is equal to this object</returns>
        public override bool Equals(object obj) => obj is Movie movie && Equals(movie);

        /// <summary>
        /// Override of the default <see cref="GetHashCode"/> method, since that is
        /// recommended by Intellicode/Roslynator.
        /// </summary>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(Deleted);
            hash.Add(UpdatedAt);
            hash.Add(Version);
            hash.Add(BestPictureWinner);
            hash.Add(Duration);
            hash.Add(Rating);
            hash.Add(ReleaseDate);
            hash.Add(Title);
            hash.Add(Year);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Clones this movie into another new movie.
        /// </summary>
        /// <returns>The new movie</returns>
        public Movie Clone() => new()
        {
            Id = this.Id,
            Deleted = this.Deleted,
            UpdatedAt = this.UpdatedAt,
            Version = this.Version,
            BestPictureWinner = this.BestPictureWinner,
            Duration = this.Duration,
            Rating = this.Rating,
            ReleaseDate = this.ReleaseDate,
            Title = this.Title,
            Year = this.Year
        };

        /// <summary>
        /// Converts this object to a dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
    }
}
