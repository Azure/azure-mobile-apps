// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class MappedMovieEntityDomainManager : MappedEntityDomainManager<Movie, MovieModel>
    {
        private MovieModelContext context;

        public MappedMovieEntityDomainManager(MovieModelContext context, HttpRequestMessage request)
            : base(context, request)
        {
            this.Request = request;
            this.context = context;
        }

        public override SingleResult<Movie> Lookup(string id)
        {
            string entityId = GetKey<string>(id);
            return this.LookupEntity(m => m.Id == entityId);
        }

        public override async Task<Movie> InsertAsync(Movie data)
        {
            return await base.InsertAsync(data);
        }

        public override Task<Movie> UpdateAsync(string id, Delta<Movie> patch)
        {
            string entityId = GetKey<string>(id);
            return this.UpdateEntityAsync(patch, entityId);
        }

        public override Task<Movie> UpdateAsync(string id, Delta<Movie> patch, bool includeDeleted)
        {
            string entityId = GetKey<string>(id);
            return this.UpdateEntityAsync(patch, includeDeleted, entityId);
        }

        public override async Task<Movie> ReplaceAsync(string id, Movie data)
        {
            return await base.ReplaceAsync(id, data);
        }

        public override Task<bool> DeleteAsync(string id)
        {
            string movieId = GetKey<string>(id);
            return this.DeleteItemAsync(movieId);
        }

        protected override void SetOriginalVersion(MovieModel model, byte[] version)
        {
            this.context.Entry(model).OriginalValues["Version"] = version;
        }
    }
}