// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using Microsoft.Zumo.Server;
using Microsoft.Zumo.Server.Entity;

namespace Microsoft.Zumo.E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class MoviesController : TableController<Movie>
    {
        public MoviesController(AppDbContext context)
        {
            TableControllerOptions = new TableControllerOptions<Movie>
            {
                MaxTop = 1000
            };
            TableRepository = new EntityTableRepository<Movie>(context);
        }

        public override int ValidateOperation(TableOperation operation, Movie item)
        {
            if (operation == TableOperation.List || operation == TableOperation.Read)
            {
                return StatusCodes.Status200OK;
            }
            return StatusCodes.Status405MethodNotAllowed;
        }
    }
}
