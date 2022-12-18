// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System.ComponentModel.DataAnnotations;

namespace Samples.Blazor.Client.Models
{
    public class TodoItemDTO : DatasyncClientData
    {
        [Required]
        [MinLength(1)]
        public string Title { get; set; } = string.Empty;

        public bool IsComplete { get; set; } = false;
    }
}
