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

namespace ZumoE2EServerApp.Controllers
{
    public class MoviesController : TableController<Movie>
    {
        private SDKClientTestContext context;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            context = new SDKClientTestContext();
            this.DomainManager = new EntityDomainManager<Movie>(context, Request);
        }

        [EnableQuery(MaxTop = 1000)]
        public IQueryable<Movie> GetAll()
        {
            return Query();
        }

        public SingleResult<Movie> Get(string id)
        {
            return Lookup(id);
        }
    }
}