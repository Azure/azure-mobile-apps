﻿using Azure.Data.Mobile;
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