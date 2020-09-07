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
    public class RoundTripTableItem : EntityTableData
    {
        public string Name { get; set; }
        public DateTimeOffset? Date1 { get; set; }
        public bool? Bool { get; set; }
        public int? Integer { get; set; }
        public double? Number { get; set; }
    }

    public class IntIdRoundTripTableItem : IInt64IdTable
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? Date1 { get; set; }

        public bool? Bool { get; set; }

        public int? Integer { get; set; }

        public double? Number { get; set; }
    }

    public class IntIdRoundTripTableItemDto : ITableData
    {
        public string Name { get; set; }

        public DateTimeOffset? Date1 { get; set; }

        public bool? Bool { get; set; }

        public int? Integer { get; set; }

        public double? Number { get; set; }

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

    public class StringIdRoundTripTableSoftDeleteItem : RoundTripTableItem
    {
    }

    public class RoundTripProfile: Profile
    {
        public RoundTripProfile()
        {
            CreateMap<IntIdRoundTripTableItem, IntIdRoundTripTableItemDto>().ForMember(dto => dto.Id,
                map => map.MapFrom(db => db.Id.ToString()));
            CreateMap<IntIdRoundTripTableItemDto, IntIdRoundTripTableItem>().ForMember(db => db.Id,
                map => map.MapFrom(dto => long.Parse(dto.Id)));
        }

    }
}