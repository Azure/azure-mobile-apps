﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Zumo.Server.Entity.Test.Helpers
{
    public class Movie : EntityTableData, IEquatable<Movie>
    {
        public Movie()
        {
            Id = Guid.NewGuid().ToString("N");
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-(180 + (new Random()).Next(180)));
            Version = Guid.NewGuid().ToByteArray();
            Deleted = false;
        }

        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }

        #region Cloneable<Movie>
        public Movie Clone()
        {
            return new Movie
            {
                Id = this.Id,
                Version = (byte[])this.Version.Clone(),
                UpdatedAt = this.UpdatedAt,
                Deleted = this.Deleted,
                Title = this.Title,
                Duration = this.Duration,
                MpaaRating = this.MpaaRating,
                ReleaseDate = this.ReleaseDate,
                BestPictureWinner = this.BestPictureWinner,
                Year = this.Year
            };
        }
        #endregion

        #region IEquatable<Movie>
        public override bool Equals(object obj)
        {
            return Equals(obj as Movie);
        }

        public bool Equals(Movie other)
        {
            // Note that we explicitly avoid using UpdatedAt & Version fields
            // in the equality check - the data must match, not the Metadata.
            return other != null &&
                   Id.Equals(other.Id) &&
                   Title == other.Title &&
                   Duration == other.Duration &&
                   MpaaRating == other.MpaaRating &&
                   ReleaseDate == other.ReleaseDate &&
                   BestPictureWinner == other.BestPictureWinner &&
                   Year == other.Year;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id);
            hash.Add(UpdatedAt);
            hash.Add(Version);
            hash.Add(Deleted);
            hash.Add(Title);
            hash.Add(Duration);
            hash.Add(MpaaRating);
            hash.Add(ReleaseDate);
            hash.Add(BestPictureWinner);
            hash.Add(Year);
            return hash.ToHashCode();
        }
        #endregion
    }
}