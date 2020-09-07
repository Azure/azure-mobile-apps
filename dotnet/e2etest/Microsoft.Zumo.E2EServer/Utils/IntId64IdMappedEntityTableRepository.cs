// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Zumo.Server;
using Microsoft.Zumo.Server.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.E2EServer.Utils
{
    public class IntIdMappedEntityTableRepository<TData, TModel> : ITableRepository<TData>
        where TData : class, ITableData
        where TModel : class, IInt64IdTable
    {
        private readonly DbContext _context;
        private readonly DbSet<TModel> _models;
        private readonly MapperConfiguration _configuration;
        private readonly IMapper _mapper;

        public IntIdMappedEntityTableRepository(DbContext context, MapperConfiguration configuration)
        {
            _context = context;
            _models = context.Set<TModel>();
            _configuration = configuration;
            _mapper = configuration.CreateMapper();
        }

        public IQueryable<TData> AsQueryable()
            => _models.ProjectTo<TData>(_configuration).AsQueryable();

        public async Task<TData> CreateAsync(TData item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            
            var entity = item.Id == null ? null : await LookupAsync(item.Id, cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                throw new EntityExistsException();
            }

            UpdateVersionFields(item);
            var tmodel = _models.Add(_mapper.Map<TModel>(item));
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<TData>(tmodel.Entity);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var entity = await LookupAsync(id, cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                _models.Remove(_mapper.Map<TModel>(entity));
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            } 
            else
            {
                throw new EntityDoesNotExistException();
            }
        }

        public async ValueTask<TData> LookupAsync(string id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            var item = await _models.FindAsync(Convert.ToInt64(id));
            return item == null ? null : _mapper.Map<TData>(item);
        }

        public async Task<TData> ReplaceAsync(TData item, CancellationToken cancellationToken = default)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentNullException(nameof(item));
            }

            var entity = await LookupAsync(item.Id, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new EntityDoesNotExistException();
            }

            UpdateVersionFields(item);
            _context.Entry(_mapper.Map<TModel>(entity)).CurrentValues.SetValues(_mapper.Map<TModel>(item));
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return await LookupAsync(item.Id, cancellationToken).ConfigureAwait(false);
        }

        private void UpdateVersionFields(TData item)
        {
            item.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
