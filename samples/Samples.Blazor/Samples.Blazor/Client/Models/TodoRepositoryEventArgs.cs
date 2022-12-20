// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Samples.Blazor.Client.Models
{
    /// <summary>
    /// Representation of an event that is generated when the repository is changed.
    /// </summary>
    public class TodoRepositoryEventArgs : EventArgs
    {
        public TodoRepositoryEventArgs(RepositoryOperation operation, TodoItemDTO? item)
        {
            Operation = operation;
            Item = item;
        }

        public RepositoryOperation Operation { get; }
        
        public TodoItemDTO? Item { get; }
    }
}
