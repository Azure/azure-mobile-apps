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
    public class MappedMoviesController : TableController<Movie>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MovieModelContext context = new MovieModelContext();
            this.DomainManager = new MappedMovieEntityDomainManager(context, Request);
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