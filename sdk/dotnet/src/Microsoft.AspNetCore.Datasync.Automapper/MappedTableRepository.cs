// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Microsoft.AspNetCore.Datasync.Automapper
{
    /// <summary>
    /// An implementation of <see cref="IRepository{TEntity}"/> that converts
    /// the <typeparamref name="Tdto"/> (which is used externally) to and from
    /// <typeparamref name="TEntity"/> (which is used internally)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="Tdto"></typeparam>
    public class MappedTableRepository<TEntity, Tdto> : IRepository<Tdto> 
        where TEntity : ITableData 
        where Tdto : ITableData
    {
        private readonly IMapper _mapper;
        private readonly IRepository<TEntity> _repository;

        /// <summary>
        /// Creates a new <see cref="MappedTableRepository{TEntity, Tdto}"/> based on a
        /// <see cref="IMapper"/> and a wrapped <see cref="IRepository{TEntity}"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IMapper"/> to use for converting between DTO and entity.</param>
        /// <param name="repository">The <see cref="IRepository{TEntity}"/> to use for storing entities.</param>
        public MappedTableRepository(IMapper mapper, IRepository<TEntity> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        #region IRepository<Tdto>
        /// <summary>
        /// Returns an unexecuted <see cref="IQueryable{T}"/> that represents the data store as a whole.
        /// This is adjusted by the <see cref="TableController{TEntity}"/> to account for filtering and
        /// paging requests.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> for the entities in the data store.</returns>
        public IQueryable<Tdto> AsQueryable()
            => _repository.AsQueryable().ProjectTo<Tdto>(_mapper.ConfigurationProvider);

        /// <summary>
        /// Creates an entity within the data store. After completion, the system properties
        /// within the entity have been updated with new values.
        /// </summary>
        /// <param name="dto">The entity to be created.</param>
        /// <param name="token">A cancellation token.</param>
        /// <exception cref="ConflictException">if the entity to be created already exists.</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task CreateAsync(Tdto dto, CancellationToken token = default)
        {
            TEntity entity = _mapper.Map<Tdto, TEntity>(dto);
            await _repository.CreateAsync(entity, token);
            _mapper.Map(entity, dto);
        }

        /// <summary>
        /// Removes an entity from the data store. If a <c>version</c> is provided, the version
        /// must match the entity version.
        /// </summary>
        /// <param name="id">The globally unique ID of the entity to be removed.</param>
        /// <param name="version">The (optional) version of the entity to be removed.</param>
        /// <param name="token">A cancellation token.</param>
        /// <exception cref="NotFoundException">if the entity does not exist.</exception>
        /// <exception cref="PreconditionFailedException">if the entity version does not match the provided version</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public Task DeleteAsync(string id, byte[]? version = null, CancellationToken token = default)
            => _repository.DeleteAsync(id, version, token);

        /// <summary>
        /// Reads the entity from the data store.
        /// </summary>
        /// <remarks>
        /// It is important that the entity returned is "disconnected" from the store. Some controller
        /// methods alter the entity to conform to the new spec.  If the entity is connected to the
        /// data store, then the data store is updated at the same time, resulting in data leakage
        /// problems.
        /// </remarks>
        /// <param name="id">The globally unique ID of the entity to be read.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>The entity, or null if the entity does not exist.</returns>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task<Tdto> ReadAsync(string id, CancellationToken token = default)
            => _mapper.Map<TEntity, Tdto>(await _repository.ReadAsync(id, token));

        /// <summary>
        /// Replace the entity within the store with the provided entity.  If a <c>version</c> is
        /// specified, then the version must match.  On return, the system properties of the entity
        /// will be updated.
        /// </summary>
        /// <param name="dto">The replacement entity.</param>
        /// <param name="version">The (optional) version of the entity to be replaced</param>
        /// <param name="token">A cancellation token</param>
        /// <exception cref="BadRequestException">if the entity does not have an ID</exception>
        /// <exception cref="NotFoundException">if the entity does not exist</exception>
        /// <exception cref="ConflictException">if the entity version does not match the provided version</exception>
        /// <exception cref="RepositoryException">if an error occurs in the data store.</exception>
        public async Task ReplaceAsync(Tdto dto, byte[]? version = null, CancellationToken token = default)
        {
            TEntity entity = _mapper.Map<Tdto, TEntity>(dto);
            await _repository.ReplaceAsync(entity, version, token);
            _mapper.Map(entity, dto);
        }
        #endregion
    }
}
