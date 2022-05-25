// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Mobile.Server;
using Newtonsoft.Json;

namespace Local.Models
{
    public class BrownOnline : EntityData
    {
        public string Item { get; set; }

        public int Quantity { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [JsonIgnore]
        public int CustomerId { get; set; }
    }
}