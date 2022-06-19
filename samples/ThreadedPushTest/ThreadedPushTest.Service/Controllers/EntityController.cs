// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;

namespace ThreadedPushTest.Service.Controllers
{
    [Route("tables/entity")]
    public class EntityController : TableController<Entity>
    {
        public EntityController(AppDbContext context)
            : base(new EntityTableRepository<Entity>(context))
        {
        }
    }
}