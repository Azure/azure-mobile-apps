// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class Movie : EntityData
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public string Rating { get; set; }

        public DateTime ReleaseDate { get; set; }

        [Range(0, 4096)]
        public int RunTimeMinutes { get; set; }

        // example of an unmapped, non-serialized computed property
        [JsonIgnore]
        [NotMapped]
        public string QuickDescription
        {
            get
            {
                return string.Format("{0} : {1}", this.Id, this.Name);
            }
        }
    }
}
