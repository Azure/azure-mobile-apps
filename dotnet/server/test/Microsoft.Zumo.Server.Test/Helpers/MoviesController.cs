// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server.Test.Helpers
{
    /// <summary>
    /// Mock controller for tests.
    /// </summary>
    public class MoviesController : TableController<Movie>
    {
        public MoviesController() : base()
        {
        }

        public MoviesController(ITableRepository<Movie> repository) : base(repository)
        {
        }

        public MoviesController(ITableRepository<Movie> repository, TableControllerOptions<Movie> options) : base(repository)
        {
            TableControllerOptions = options;
        }

        #region Base Access
        // Calling the BaseX (for each X that is overridden) is allowed for tests.
        public bool BaseIsAuthorized(TableOperation operation, Movie item) => base.IsAuthorized(operation, item);

        public int BaseValidateOperation(TableOperation operation, Movie item) => base.ValidateOperation(operation, item);

        public Task<int> BaseValidateOperationAsync(TableOperation operation, Movie item) => base.ValidateOperationAsync(operation, item);

        public Movie BasePrepareItemForStore(Movie item) => base.PrepareItemForStore(item);

        public Task<Movie> BasePrepareItemForStoreAsync(Movie item) => base.PrepareItemForStoreAsync(item);
        #endregion

        #region Overrides
        public bool IsAuthorizedResult { get; set; } = true;
        public int IsAuthorizedCount { get; private set; } = 0;
        public TableOperation LastAuthorizedOperation { get; private set; }
        public Movie LastAuthorizedMovie { get; private set; }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            IsAuthorizedCount++;
            LastAuthorizedOperation = operation;
            LastAuthorizedMovie = item;
            return IsAuthorizedResult;
        }

        public int? ValidateOperationResult { get; set; }
        public int ValidateOperationCount { get; private set; } = 0;
        public TableOperation LastValidateOperation { get; private set; }
        public Movie LastValidateOperationMovie { get; private set; }
        public bool WasLastValidateOperationAsync { get; private set; } = false;

        public override int ValidateOperation(TableOperation operation, Movie item)
        {
            if (ValidateOperationResult == null)
            {
                return base.ValidateOperation(operation, item);
            } else
            {
                ValidateOperationCount++;
                LastValidateOperation = operation;
                LastValidateOperationMovie = item;
                WasLastValidateOperationAsync = false;
                return (int)ValidateOperationResult;
            }
        }

        public int? ValidateOperationAsyncResult { get; set; }

        public override Task<int> ValidateOperationAsync(TableOperation operation, Movie item, CancellationToken cancellationToken = default)
        {
            if (ValidateOperationAsyncResult == null)
            {
                return base.ValidateOperationAsync(operation, item, cancellationToken);
            }
            else
            {
                ValidateOperationCount++;
                LastValidateOperation = operation;
                LastValidateOperationMovie = item;
                WasLastValidateOperationAsync = true;
                return Task.FromResult((int)ValidateOperationAsyncResult);
            }
        }
        #endregion
    }
}
