﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace DeviceTests.Shared.Helpers.Models
{
    public interface IMovie
    {
        string Title { get; set; }
        int Duration { get; set; }
        string MpaaRating { get; set; }
        DateTime ReleaseDate { get; set; }
        bool BestPictureWinner { get; set; }
        int Year { get; set; }
    }

    [DataTable("intIdMovies")]
    public class IntIdMovie : IMovie
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Movie[Title={0},Duration={1},Rating={2},ReleaseDate={3},BestPictureWinner={4},Year={5}",
                Title, Duration, MpaaRating,
                ReleaseDate.ToUniversalTime().ToString("yyyy-MM-dd:HH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                BestPictureWinner, Year);
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (Title != null) result ^= Title.GetHashCode();
            result ^= Duration.GetHashCode();
            if (MpaaRating != null) result ^= MpaaRating.GetHashCode();
            result ^= ReleaseDate.ToUniversalTime().GetHashCode();
            result ^= BestPictureWinner.GetHashCode();
            result ^= Year.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            IntIdMovie other = obj as IntIdMovie;
            if (other == null) return false;
            if (this.Title != other.Title) return false;
            if (this.Duration != other.Duration) return false;
            if (this.MpaaRating != other.MpaaRating) return false;
            if (!this.ReleaseDate.ToUniversalTime().Equals(other.ReleaseDate.ToUniversalTime())) return false;
            if (this.BestPictureWinner != other.BestPictureWinner) return false;
            if (this.Year != other.Year) return false;
            return true;
        }
    }

    [DataTable("Movies")]
    public class Movie : IMovie
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }

        public Movie() { }
        public Movie(string id, IntIdMovie movie)
        {
            this.Id = id;
            this.Title = movie.Title;
            this.Duration = movie.Duration;
            this.MpaaRating = movie.MpaaRating;
            this.ReleaseDate = movie.ReleaseDate;
            this.BestPictureWinner = movie.BestPictureWinner;
            this.Year = movie.Year;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Movie/string id[Title={0},Duration={1},Rating={2},ReleaseDate={3},BestPictureWinner={4},Year={5}",
                Title, Duration, MpaaRating,
                ReleaseDate.ToUniversalTime().ToString("yyyy-MM-dd:HH:mm:ss.fffZ", CultureInfo.InvariantCulture),
                BestPictureWinner, Year);
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (Title != null) result ^= Title.GetHashCode();
            result ^= Duration.GetHashCode();
            if (MpaaRating != null) result ^= MpaaRating.GetHashCode();
            result ^= ReleaseDate.ToUniversalTime().GetHashCode();
            result ^= BestPictureWinner.GetHashCode();
            result ^= Year.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            Movie other = obj as Movie;
            if (other == null) return false;
            if (this.Title != other.Title) return false;
            if (this.Duration != other.Duration) return false;
            if (this.MpaaRating != other.MpaaRating) return false;
            if (!this.ReleaseDate.ToUniversalTime().Equals(other.ReleaseDate.ToUniversalTime())) return false;
            if (this.BestPictureWinner != other.BestPictureWinner) return false;
            if (this.Year != other.Year) return false;
            return true;
        }
    }

    [DataTable("intIdMovies")]
    public class AllIntIdMovies
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "movies")]
        public IntIdMovie[] Movies { get; set; }
    }

    [DataTable("movies")]
    public class AllMovies
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "movies")]
        public Movie[] Movies { get; set; }
    }
}
