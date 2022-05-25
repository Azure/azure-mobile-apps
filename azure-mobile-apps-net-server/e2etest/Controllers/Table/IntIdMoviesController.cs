// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    public class IntIdMoviesController : TableController<IntIdMovieDto>
    {
        private SDKClientTestContext context;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            context = new SDKClientTestContext();
            this.DomainManager = new Int64IdMappedEntityDomainManager<IntIdMovieDto, IntIdMovie>(context, Request);
        }

        [EnableQuery(MaxTop = 1000, EnsureStableOrdering = false)]
        public IQueryable<IntIdMovieDto> GetAll()
        {
            return Query();
        }

        public SingleResult<IntIdMovieDto> Get(string id)
        {
            return Lookup(id);
        }
    }
}