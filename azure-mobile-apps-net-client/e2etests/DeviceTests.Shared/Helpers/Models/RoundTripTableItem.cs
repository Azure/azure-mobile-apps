// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace DeviceTests.Shared.Helpers.Models
{
    [DataTable("RoundTripTable")]
    public class RoundTripTableItem : ICloneableItem<RoundTripTableItem>
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "date1")]
        public DateTime? Date { get; set; }

        [JsonProperty(PropertyName = "bool")]
        public bool? Bool { get; set; }

        [JsonProperty(PropertyName = "integer")]
        public int Integer { get; set; }

        [JsonProperty(PropertyName = "number")]
        public double Number { get; set; }

        public RoundTripTableItem() { }
        public RoundTripTableItem(Random rndGen)
        {
            this.Name = CreateSimpleRandomString(rndGen, 5);
            this.Date = new DateTime(rndGen.Next(1980, 2000), rndGen.Next(1, 12), rndGen.Next(1, 25), rndGen.Next(0, 24), rndGen.Next(0, 60), rndGen.Next(0, 60), DateTimeKind.Utc);
            this.Bool = rndGen.Next(2) == 0;
            this.Integer = rndGen.Next();
            this.Number = rndGen.Next(10000) * rndGen.NextDouble();
        }

        object ICloneableItem<RoundTripTableItem>.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        public RoundTripTableItem Clone()
        {
            var result = new RoundTripTableItem
            {
                Id = this.Id,
                Bool = this.Bool,
                Date = this.Date,
                Integer = this.Integer,
                Number = this.Number,
                Name = this.Name,
            };

            return result;
        }

        public override bool Equals(object obj)
        {
            const double acceptableDifference = 1e-6;
            var other = obj as RoundTripTableItem;
            if (other == null) return false;
            if (!this.Bool.Equals(other.Bool)) return false;
            if (this.Date.HasValue != other.Date.HasValue) return false;
            if (this.Date.HasValue && !this.Date.Value.ToUniversalTime().Equals(other.Date.Value.ToUniversalTime())) return false;
            if (this.Integer != other.Integer) return false;
            if (Math.Abs(this.Number - other.Number) > acceptableDifference) return false;
            if (this.Name != other.Name) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "RoundTripTableItem[Bool={0},Date1={1},Integer={2},Number={3},Name={4}]",
                Bool.HasValue ? Bool.Value.ToString() : "<<NULL>>",
                Date.HasValue ? Date.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff") : "<<NULL>>",
                Integer,
                Number,
                Name);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Bool.GetHashCode();

            if (this.Date.HasValue)
            {
                result ^= this.Date.Value.ToUniversalTime().GetHashCode();
            }

            result ^= this.Integer.GetHashCode();
            result ^= this.Number.GetHashCode();

            if (this.Name != null)
            {
                result ^= this.Name.GetHashCode();
            }

            return result;
        }

        public static string CreateSimpleRandomString(Random rndGen, int size)
        {
            return new string(
                Enumerable.Range(0, size)
                    .Select(_ => (char)rndGen.Next(' ', '~' + 1))
                    .ToArray());
        }

        public static int GetArrayHashCode<T>(T[] array)
        {
            int result = 0;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object item = array.GetValue(i);
                    if (item != null)
                    {
                        result ^= item.GetHashCode();
                    }
                }
            }

            return result;
        }

    }
}
