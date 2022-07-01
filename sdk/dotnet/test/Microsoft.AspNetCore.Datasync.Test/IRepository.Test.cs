// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test
{
    public class IRepository_Tests
    {
        [Fact]
        public async Task IRepository_AsQueryableAsync_HasDefault()
        {
            var repository = new ConcreteRepository();
            var iRepository = (IRepository<EFMovie>)repository;

            var count = (await iRepository.AsQueryableAsync()).Count();

            Assert.Equal(0, count);
            Assert.True(repository.HasCalled);
        }
    }

    [ExcludeFromCodeCoverage]
    internal class ConcreteRepository : IRepository<EFMovie>
    {
        private readonly EFMovie[] _movies = Array.Empty<EFMovie>();

        public bool HasCalled { get; set; } = false;

        public IQueryable<EFMovie> AsQueryable()
        {
            HasCalled = true;
            return _movies.AsQueryable();
        }

        public Task CreateAsync(EFMovie entity, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(string id, byte[] version = null, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<EFMovie> ReadAsync(string id, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task ReplaceAsync(EFMovie entity, byte[] version = null, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
