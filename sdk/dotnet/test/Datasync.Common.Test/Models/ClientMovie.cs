// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.Datasync.Client;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datasync.Common.Test.Models;

public interface ISystemData
{
    string Id { get; set; }
    bool Deleted { get; set; }
}

[ExcludeFromCodeCoverage]
public class ClientMovie : DatasyncClientData, ISystemData, IMovie, IEquatable<IMovie>
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

    /// <summary>
    /// Creates a <see cref="ClientMovie"/> from a server object
    /// </summary>
    /// <param name="source">The source object</param>
    /// <returns>The client object</returns>
    public static ClientMovie From(object source)
    {
        var entity = new ClientMovie();
        if (source is IMovie movie)
        {
            entity.BestPictureWinner = movie.BestPictureWinner;
            entity.Duration = movie.Duration;
            entity.Rating = movie.Rating;
            entity.ReleaseDate = movie.ReleaseDate;
            entity.Title = movie.Title;
            entity.Year = movie.Year;
        }

        if (source is ITableData tableData)
        {
            entity.Id = tableData.Id;
            entity.UpdatedAt = tableData.UpdatedAt;
            entity.Version = Convert.ToBase64String(tableData.Version);
            entity.Deleted = tableData.Deleted;
        }

        return entity;
    }

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
