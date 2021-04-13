// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using aspnetcore.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AzureMobile.Server;
using Microsoft.AzureMobile.Server.EFCore;

namespace aspnetcore.Controllers
{
    [Route("tables/[controller]")]
    public class TodoItemController : TableController<TodoItem>
    {
        public TodoItemController(AppDbContext context) : base()
        {
            Repository = new EntityTableRepository<TodoItem>(context);
        }
    }
}
