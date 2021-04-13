// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AzureMobile.Server.EFCore;

namespace aspnetcore.Db
{
    /// <summary>
    /// A DTO (data transfer object) for the TodoItems table.
    /// </summary>
    public class TodoItem : EntityTableData
    {
        /// <summary>
        /// Text of the Todo Item
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Is the item complete?
        /// </summary>
        public bool Complete { get; set; }
    }
}
