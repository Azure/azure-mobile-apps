using Azure.Mobile.Server.Entity;
using System;

namespace Azure.Mobile.Server.Test.Helpers
{
    public class ValidationController: TableController<Movie>
    {
        public ValidationController() : base() { }

        public ValidationController(MovieDbContext context) : base(new EntityTableRepository<Movie>(context)) { }

        public override int ValidateOperation(TableOperation operation, Movie item)
        {
            LastValidationOperation = operation;
            LastValidationItem = item;
            return 418; /* I'm a teapot */
        }

        public TableOperation LastValidationOperation { get; set; }
        public Movie LastValidationItem { get; set; }

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
