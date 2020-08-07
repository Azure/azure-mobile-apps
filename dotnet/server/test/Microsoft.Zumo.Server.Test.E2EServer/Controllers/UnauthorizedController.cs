// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using Microsoft.Zumo.Server.Test.E2EServer.Database;
using Microsoft.Zumo.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Zumo.Server.Test.E2EServer.Controllers
{
    [Route("tables/unauthorized")]
    [ApiController]
    public class UnauthorizedController : TableController<Movie>
    {
        public UnauthorizedController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<Movie>(context);
        }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            return HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
