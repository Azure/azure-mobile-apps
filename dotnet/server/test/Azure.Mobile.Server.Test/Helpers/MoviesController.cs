using Azure.Mobile.Server.Entity;
using System;

namespace Azure.Mobile.Server.Test.Helpers
{
    public class MoviesController : TableController<Movie>
    {
        public MoviesController() : base() { }

        public MoviesController(MovieDbContext context) : base(new EntityTableRepository<Movie>(context)) { }

        #region IsAuthorized
        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            IsAuthorizedCallCount++;
            LastAuthorizationOperation = operation;
            LastAuthorizationItem = item;
            return IsAuthorizedResult;
        }

        public bool BaseIsAuthorized(TableOperation operation, Movie item) => base.IsAuthorized(operation, item);

        public int IsAuthorizedCallCount { get; private set; } = 0;
        public TableOperation LastAuthorizationOperation { get; private set; }
        public Movie LastAuthorizationItem { get; private set; }
        public bool IsAuthorizedResult { get; set; } = true;
        #endregion

        #region PrepareItemForStore
        public Movie BasePrepareItemForStore(Movie item) => base.PrepareItemForStore(item);

        public override Movie PrepareItemForStore(Movie item)
        {
            PrepareItemForStoreCallCount++;
            LastPreparedItem = item;
            return PrepareItemFunc(item);
        }

        public int PrepareItemForStoreCallCount { get; private set; } = 0;
        public Movie LastPreparedItem { get; private set; }
        public Func<Movie, Movie> PrepareItemFunc { get; set; } = movie => movie;
        #endregion
    }
}
