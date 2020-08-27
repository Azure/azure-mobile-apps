// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using Microsoft.Zumo.Server;
using Microsoft.Zumo.Server.Entity;

namespace Microsoft.Zumo.E2EServer.Controllers
{
    [Route("tables/blog_comments")]
    [ApiController]
    public class BlogCommentsController : TableController<BlogComment>
    {
        public BlogCommentsController(AppDbContext context)
        {
            TableRepository = new EntityTableRepository<BlogComment>(context);
        }
    }
}
