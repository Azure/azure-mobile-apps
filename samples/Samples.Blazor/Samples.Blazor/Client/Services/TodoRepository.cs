// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Samples.Blazor.Client.Models;

namespace Samples.Blazor.Client.Services
{
    public class TodoRepository : ITodoRepository
    {
        private HttpClient _httpClient;

        public TodoRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task AddNewItemAsync(TodoItemDTO item)
        {
            return Task.CompletedTask;
        }
    }
}
