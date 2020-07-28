using Azure.Mobile.Server.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Entity
{
    /// <summary>
    /// Implementation of the <see cref="ITableRepository{TEntity}"/> interface that uses Entity
    /// Framework Core  for accessing a backend store.
    /// </summary>
    /// <typeparam name="TEntity">The model type for stored entities</typeparam>
    public class EntityTableRepository<TEntity> : ITableRepository<TEntity> where TEntity : EntityTableData
    {
        /// <summary>
        /// The EF Core DbContext for requests to the backend.
        /// </summary>
        private DbContext Context { get; set; }

        /// <summary>
        /// The EF Core data set.
        /// </summary>
        private DbSet<TEntity> DataSet { get; set; }

        /// <summary>
        /// Create a new <see cref="EntityTableRepository{TEntity}"/> with the specified DbContext. This
        /// is the normal ctor for the table repository.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> for the backend store.</param>
        public EntityTableRepository(DbContext context)
        {
            Context = context;
            DataSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Builds an <see cref="IQueryable{T}"/> to be executed against a store supporting LINQ for querying data.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> that has not yet been executed.</returns>
        public virtual IQueryable<TEntity> AsQueryable()
            => DataSet.AsQueryable();

        /// <summary>
        /// Creates a new entity within the store.  The entity must not exist.  If the entity exists, then <see cref="EntityExistsException"/>
        /// is thrown.
        /// </summary>
        /// <param name="item">The new form of the entity</param>
        /// <returns>The new entity</returns>
        /// <exception cref="EntityExistsException">if the entity exists</exception>
        public virtual async Task<TEntity> CreateAsync(TEntity item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var entity = await LookupAsync(item.Id, cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                throw new EntityExistsException();
            }

            UpdateVersionFields(item);
            DataSet.Add(item);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return item;
        }

        /// <summary>
        /// Deletes the entity within the store. 
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <exception cref="EntityDoesNotExistException">if the entity to be deleted does not exist</exception>
        public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var entity = await LookupAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                DataSet.Remove(entity);
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new EntityDoesNotExistException();
            }
        }

        /// <summary>
        /// Looks up a single entity in the store.
        /// </summary>
        /// <param name="id">The id of the entity to be returned</param>
        /// <returns>The entity, or null if no entity is available.</returns>
        public virtual ValueTask<TEntity> LookupAsync(string id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            return DataSet.FindAsync(new string[] { id }, cancellationToken);
        }

        /// <summary>
        /// Replaces an entity within the store.  The entity must exist.
        /// </summary>
        /// <param name="item">The new form of the entity</param>
        /// <returns>The new entity</returns>
        /// <exception cref="EntityDoesNotExistException">if the entity to be replaced does not exist</exception>
        public virtual async Task<TEntity> ReplaceAsync(TEntity item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (item.Id == null)
            {
                throw new ArgumentNullException(nameof(item.Id));
            }

            var entity = await LookupAsync(item.Id, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new EntityDoesNotExistException();
            }

            UpdateVersionFields(item);
            Context.Entry(entity).CurrentValues.SetValues(item);
            await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// We cannot rely on all EF Core drivers actually updating records, so we will do it ourselves.
        /// </summary>
        /// <param name="item">The entity to be updated.</param>
        private void UpdateVersionFields(TEntity item)
        {
            item.UpdatedAt = DateTimeOffset.UtcNow;
            item.Version = Guid.NewGuid().ToByteArray();
        }
    }
}
