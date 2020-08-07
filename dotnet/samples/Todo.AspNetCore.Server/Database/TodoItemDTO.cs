// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using System;
using System.Text.Json.Serialization;

namespace Todo.AspNetCore.Server.Database
{
    /// <summary>
    /// The Data Transfer Object (DTO) for the TodoItem.
    /// </summary>
    public class TodoItemDTO : EntityTableData
    {
        /// <summary>
        /// The Owner of the TodoItem
        /// </summary>
        [JsonIgnore]
        public string OwnerId { get; set; }

        /// <summary>
        /// The text or title of the TodoItem
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional field to allow the user to specify a category
        /// or color for the TodoItem
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// True if the TodoItem is completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The date and time the TodoItem is due
        /// </summary>
        public DateTimeOffset Due { get; set; }
    }
}
