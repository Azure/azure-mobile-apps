using Azure.Mobile.Client;
using System;

namespace Todo.NetStandard.Common
{
    /// <summary>
    /// Model for a single TodoItem.
    /// </summary>
    public class TodoItem : TableData
    {
        /// <summary>
        /// The text or title of the TodoItem
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// True if the TodoItem is completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The time the task is due
        /// </summary>
        public DateTime Due { get; set; }
    }
}
