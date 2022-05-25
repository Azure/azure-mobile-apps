// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Local.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public string Item { get; set; }

        public int Quantity { get; set; }

        public virtual Customer Customer { get; set; }

        public int CustomerId { get; set; }

        [TableColumn(TableColumnType.Version)]
        [Timestamp]
        public byte[] Version { get; set; }

        [TableColumn(TableColumnType.CreatedAt)]
        public DateTimeOffset? CreatedAt { get; set; }

        [TableColumn(TableColumnType.UpdatedAt)]
        public DateTimeOffset? UpdatedAt { get; set; }

        [TableColumn(TableColumnType.Deleted)]
        public bool Deleted { get; set; }
    }
}