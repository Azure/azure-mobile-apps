// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using TodoApp.Data.Services;

namespace TodoApp.Data
{
    /// <summary>
    /// A static and thread safe version to get the TodoService.
    /// </summary>
    public static class TodoService
    {
        private static readonly Lazy<ITodoService> _serviceRef = new(() => new RemoteTodoService());

        /// <summary>
        /// The <see cref="ITodoService"/> instance.
        /// </summary>
        public static ITodoService Value { get => _serviceRef.Value; }
    }
}
