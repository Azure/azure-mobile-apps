// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using Microsoft.Zumo.Server.Test.E2EServer.Database;
using Microsoft.Zumo.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Zumo.Server.Test.E2EServer.Controllers
{
    [Route("tables/blog_comments")]
    [ApiController]
    public class BlogCommentsController : TableController<BlogComment>
    {
        public BlogCommentsController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<BlogComment>(context);
        }
    }
}
