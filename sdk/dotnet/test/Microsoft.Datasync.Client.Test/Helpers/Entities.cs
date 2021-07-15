// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public class IdEntity : IEquatable<IdEntity>
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string StringValue { get; set; }

        public string StringField;

        public bool Equals(IdEntity other) => Id == other.Id && StringValue == other.StringValue;
        public override bool Equals(object obj) => obj is IdEntity ide && Equals(ide);
        public override int GetHashCode() => Id.GetHashCode() + StringValue.GetHashCode();
    }

    [ExcludeFromCodeCoverage]
    public class KeyEntity
    {
        [Id]
        public string KeyId { get; set; }
        public string KeyVersion { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class NoIdEntity
    {
        public string Test { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class NonStringIdEntity
    {
        public bool Id { get; set; }
        public bool Version { get; set; }
    }

    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Movie : DatasyncClientData
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
        [JsonPropertyName("rating")]
        public string MpaaRating { get; set; }

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
            && other.BestPictureWinner == BestPictureWinner
            && other.Duration == Duration
            && other.MpaaRating == MpaaRating
            && other.ReleaseDate == ReleaseDate
            && other.Title == Title
            && other.Year == Year;

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
            MpaaRating = this.MpaaRating,
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
