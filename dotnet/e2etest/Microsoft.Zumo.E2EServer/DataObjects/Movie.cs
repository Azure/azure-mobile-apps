// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AutoMapper;
using Microsoft.Zumo.E2EServer.Utils;
using Microsoft.Zumo.Server;
using Microsoft.Zumo.Server.Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.Zumo.E2EServer.DataObjects
{
    public class Movie : EntityTableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }

    public class IntIdMovie : IInt64IdTable
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }

    public class IntIdMovieDto : ITableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }

        public string Id { get; set; }

        [NotMapped]
        public bool Deleted { get; set; }

        [NotMapped]
        public DateTimeOffset UpdatedAt { get; set; }

        [NotMapped]
        public DateTimeOffset CreatedAt { get; set; }

        [NotMapped]
        public Byte[] Version { get; set; }
    }

    public class MovieProfile : Profile
    {
        public MovieProfile()
        {
            CreateMap<IntIdMovie, IntIdMovieDto>().ForMember(dto => dto.Id,
                map => map.MapFrom(db => db.Id.ToString()));
            CreateMap<IntIdMovieDto, IntIdMovie>().ForMember(db => db.Id,
                map => map.MapFrom(dto => long.Parse(dto.Id)));
        }
    }

}
