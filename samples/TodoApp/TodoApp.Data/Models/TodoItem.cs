// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System;

namespace TodoApp.Data.Models
{
    /// <summary>
    /// The model that is for the TodoItem pulled from the service.  This must
    /// match what is coming from the service.
    /// </summary>
    public class TodoItem : DatasyncClientData, IEquatable<TodoItem>
    {
        public string Title { get; set; }

        public bool IsComplete { get; set; }

        public bool Equals(TodoItem other)
            => other != null && other.Id == Id && other.Title == Title && other.IsComplete == IsComplete;
    }
}
