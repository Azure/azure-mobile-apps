// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.TestModels;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    public class MoviesController : TableController<Movie>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MovieContext context = new MovieContext();
            this.DomainManager = new EntityDomainManager<Movie>(context, Request);
            this.Request.RegisterForDispose(context);
        }

        public IQueryable<Movie> GetMovies()
        {
            return TestData.Movies.AsQueryable<Movie>();
        }

        public SingleResult<Movie> GetMovie(string id)
        {
            return SingleResult.Create(TestData.Movies.Where(p => p.Id == id).AsQueryable());
        }
    }
}