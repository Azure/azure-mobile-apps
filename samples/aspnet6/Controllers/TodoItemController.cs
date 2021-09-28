// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using aspnet6.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;

namespace aspnet6.Controllers
{
    [AllowAnonymous]
    [Route("tables/[controller]")]
    public class TodoItemController : TableController<TodoItem>
    {
        public TodoItemController(AppDbContext context) : base()
        {
            Repository = new EntityTableRepository<TodoItem>(context);
        }
    }
}
