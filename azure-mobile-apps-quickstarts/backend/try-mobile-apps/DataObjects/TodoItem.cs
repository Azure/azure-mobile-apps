using Microsoft.Azure.Mobile.Server;
using System;

namespace TryMobileAppsService.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }
        public bool Complete { get; set; }

        // SQL Compact Edition does not support the DateTimeOffset type.
        // These are simple backing properties that store these values using the DateTime type instead.
        public DateTime __createdAtDateTime
        {
            get { return CreatedAt.HasValue ? CreatedAt.Value.DateTime : DateTime.Now; }
            set { CreatedAt = value; }
        }

        public DateTime __updatedAtDateTime
        {
            get { return UpdatedAt.HasValue ? UpdatedAt.Value.DateTime : DateTime.Now; }
            set { UpdatedAt = value; }
        }
    }
}