using Azure.Mobile.Server.Entity;
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
        /// True if the TodoItem is completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The date/time that the task is due.
        /// </summary>
        public DateTime Due { get; set; }
    }
}
