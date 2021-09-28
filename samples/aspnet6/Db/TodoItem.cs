// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;
using System.ComponentModel.DataAnnotations;

namespace aspnet6.Db
{
    /// <summary>
    /// A DTO (data transfer object) for the TodoItems table.
    /// </summary>
    public class TodoItem : EntityTableData
    {
        /// <summary>
        /// Text of the Todo Item
        /// </summary>
        [Required]
        public string? Text { get; set; } 

        /// <summary>
        /// Is the item complete?
        /// </summary>
        public bool Complete { get; set; }
    }
}
