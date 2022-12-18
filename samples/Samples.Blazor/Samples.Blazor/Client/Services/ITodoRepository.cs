// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Samples.Blazor.Client.Models;

namespace Samples.Blazor.Client.Services
{
    public interface ITodoRepository
    {
        Task AddNewItemAsync(TodoItemDTO item);
    }
}
