using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mobile.Common.Test.Controllers
{
    /// <summary>
    /// This is the test controller that is used (and linked) to the TestHost
    /// The user is always unauthorized.
    /// </summary>
    [Route("/tables/[controller]")]
    [ApiController]
    public class UnAuthorizedController: TableController<Movie>
    {
        public UnAuthorizedController(MovieDbContext context)
        {
            TableRepository = new EntityTableRepository<Movie>(context);
        }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            return false;
        }
    }
}
