// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Microsoft.AspNetCore.Datasync.Automapper;

/// <summary>
/// An implementation of the <see cref="IRepository{TEntity}"/> that converts the
/// <typeparamref name="Tdto"/> (which is used externally) to and from the
/// <typeparamref name="Tentity"/> (which is used internally).
/// </summary>
/// <typeparamref name="TEntity">The type used by the inner repository for storage to the database.</typeparamref>
/// <typeparamref name="Tdto">The type used by the table controller for communications.</typeparamref>
public class MappedTableRepository<TEntity, Tdto> : IRepository<Tdto>
    where TEntity : ITableData
    where Tdto : ITableData
{
    private readonly IMapper _mapper;
    private readonly IRepository<TEntity> _repository;

    /// <summary>
    /// Creates a new <see cref="MappedTableRepository{TEntity, Tdto}"/> based on an
    /// <see cref="IMapper"/> and an inner <see cref="IRepository{TEntity}"/>.
    /// </summary>
    /// <param name="mapper">The <see cref="IMapper"/> to use for converting between DTO and entity types.</param>
    /// <param name="repository">The <see cref="IRepository{TEntity}"/> to use for storing the entities.</param>
    public MappedTableRepository(IMapper mapper, IRepository<TEntity> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    #region IRepository<Tdto> implementation
    /// <inheritdoc />
    public async ValueTask<IQueryable<Tdto>> AsQueryableAsync(CancellationToken cancellationToken = default)
    {
        var queryable = await _repository.AsQueryableAsync(cancellationToken).ConfigureAwait(false);
        return queryable.ProjectTo<Tdto>(_mapper.ConfigurationProvider);
    }

    /// <inheritdoc />
    public async ValueTask CreateAsync(Tdto dto, CancellationToken cancellationToken = default)
    {
        TEntity entity = _mapper.Map<Tdto, TEntity>(dto);
        await _repository.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        _mapper.Map(entity, dto);
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, version, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<Tdto> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);
        return _mapper.Map<TEntity, Tdto>(entity);
    }

    /// <inheritdoc />
    public async ValueTask ReplaceAsync(Tdto dto, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        TEntity entity = _mapper.Map<Tdto, TEntity>(dto);
        await _repository.ReplaceAsync(entity, version, cancellationToken);
        _mapper.Map(entity, dto);
    }
    #endregion
}