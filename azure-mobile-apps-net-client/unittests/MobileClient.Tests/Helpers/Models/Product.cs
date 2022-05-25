// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MobileClient.Tests.Helpers
{
    public class Product
    {
        public long Id { get; set; }
        public int SmallId { get; set; }
        public ulong UnsignedId { get; set; }
        public uint UnsignedSmallId { get; set; }
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public float Weight { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public float? WeightInKG { get; set; }

        public Decimal Price { get; set; }
        public bool InStock { get; set; }
        public short DisplayAisle { get; set; }
        public ushort UnsignedDisplayAisle { get; set; }
        public byte OptionFlags { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public TimeSpan AvailableTime { get; set; }
        public List<string> Tags { get; set; }
        public ProductType Type { get; set; }

        public Product()
        {
        }

        public Product(long id)
        {
            this.Id = id;
        }
    }

    public class ProductWithDateTimeOffset
    {
        public long Id { get; set; }
        public int SmallId { get; set; }
        public ulong UnsignedId { get; set; }
        public uint UnsignedSmallId { get; set; }
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public float Weight { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public float? WeightInKG { get; set; }

        public Decimal Price { get; set; }
        public bool InStock { get; set; }
        public short DisplayAisle { get; set; }
        public ushort UnsignedDisplayAisle { get; set; }
        public byte OptionFlags { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public TimeSpan AvailableTime { get; set; }
        public List<string> Tags { get; set; }
        public ProductType Type { get; set; }

        public ProductWithDateTimeOffset()
        {
        }

        public ProductWithDateTimeOffset(long id)
        {
            this.Id = id;
        }
    }

    public enum ProductType
    {
        Food,
        Furniture,
    }
}
