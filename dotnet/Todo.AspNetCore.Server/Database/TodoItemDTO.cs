using Azure.Mobile.Server.Entity;
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
        public string Text { get; set; }

        /// <summary>
        /// True if the TodoItem is completed.
        /// </summary>
        public bool Complete { get; set; }
    }
}
