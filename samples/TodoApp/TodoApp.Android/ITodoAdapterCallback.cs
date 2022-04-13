// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using TodoApp.Data.Models;

namespace TodoApp.Android
{
    /// <summary>
    /// An interface used when the adapter wishes to update an item.
    /// </summary>
    public interface ITodoAdapterCallback
    {
        Task UpdateItemFromListAsync(TodoItem item, bool isChecked);
    }
}