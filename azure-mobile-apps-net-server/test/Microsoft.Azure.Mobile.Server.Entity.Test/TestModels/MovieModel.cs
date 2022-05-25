// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    /// <summary>
    /// The <see cref="MovieModel"/> is used as the domain model in <see cref="MappedEntityDomainManagerTests"/>.
    /// </summary>
    public class MovieModel
    {
        [Key]
        public string Id { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Rating { get; set; }

        public DateTime ReleaseDate { get; set; }

        [Range(0, 4096)]
        public int RunTimeMinutes { get; set; }

        public bool Deleted { get; set; }
    }
}
