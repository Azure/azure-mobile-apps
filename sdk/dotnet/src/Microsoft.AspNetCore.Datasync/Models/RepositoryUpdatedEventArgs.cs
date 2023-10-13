using System;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// The event arguments for the <see cref="TableController{TEntity}.RepositoryUpdated"/> event.
    /// </summary>
    public class RepositoryUpdatedEventArgs
    {
        /// <summary>
        /// Creates a new set of <see cref="RepositoryUpdatedEventArgs"/>.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="entityName"></param>
        /// <param name="entity"></param>
        public RepositoryUpdatedEventArgs(TableOperation operation, string entityName, object entity)
        {
            Operation = operation;
            EntityName = entityName;
            Entity = entity;
        }

        /// <summary>
        /// The operation that was performed.
        /// </summary>
        public TableOperation Operation { get; }

        /// <summary>
        /// The name of the entity.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// The value of the (updated; except for Delete) entity.
        /// </summary>
        public object Entity { get; }

        /// <summary>
        /// The time the repository event was raised.
        /// </summary>
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
    }
}
