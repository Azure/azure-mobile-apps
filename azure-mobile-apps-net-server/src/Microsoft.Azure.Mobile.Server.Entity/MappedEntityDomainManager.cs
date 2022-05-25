// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// Provides an <see cref="DomainManager{T}"/> implementation targeting SQL as the backend store using
    /// Entity Framework where there is not a 1:1 mapping between the data object (DTO) exposed through
    /// a <see cref="TableController{T}"/> and the domain model managed by SQL.
    /// See <see cref="EntityDomainManager{T}"/> for situations where there is a 1:1 relationship between
    /// the Data Object (DTO) and the domain model managed by SQL.
    /// </summary>
    /// <remarks>
    /// The <see cref="MappedEntityDomainManager{TData,TModel}"/> leverages AutoMapper to map between the
    /// DTO and the domain model and it is assumed that AutoMapper has already been initialized with
    /// appropriate mappings that map from DTO => domain model and from domain model => DTO. The
    /// bi-directional mapping is required for both reads (GET, QUERY) and updates (PUT, POST, DELETE, PATCH)
    /// to function.
    /// </remarks>
    /// <typeparam name="TData">The data object (DTO) type.</typeparam>
    /// <typeparam name="TModel">The type of the domain data model</typeparam>
    public abstract class MappedEntityDomainManager<TData, TModel> : DomainManager<TData>
        where TData : class, ITableData
        where TModel : class
    {
        private DbContext context;

        /// <summary>
        /// Creates a new instance of <see cref="MappedEntityDomainManager{TData, TModel}"/>
        /// </summary>
        /// <param name="context">
        /// An instance of <see cref="DbContext"/>
        /// </param>
        /// <param name="request">
        /// An instance of <see cref="HttpRequestMessage"/>
        /// </param>
        protected MappedEntityDomainManager(DbContext context, HttpRequestMessage request)
            : this(context, request, enableSoftDelete: false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="MappedEntityDomainManager{TData, TModel}"/>
        /// </summary>
        /// <param name="context">
        /// An instance of <see cref="DbContext"/>
        /// </param>
        /// <param name="request">
        /// An instance of <see cref="HttpRequestMessage"/>
        /// </param>
        /// <param name="enableSoftDelete">
        /// Determines whether rows are hard deleted or marked as deleted.
        /// </param>
        protected MappedEntityDomainManager(DbContext context, HttpRequestMessage request, bool enableSoftDelete)
            : base(request, enableSoftDelete)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.context = context;
        }

        public DbContext Context
        {
            get
            {
                return this.context;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.context = value;
            }
        }

        public override IQueryable<TData> Query()
        {
            IQueryable<TData> query = this.Context.Set<TModel>().ProjectTo<TData>();
            query = TableUtils.ApplyDeletedFilter(query, this.IncludeDeleted);
            return query;
        }

        public override Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query)
        {
            throw TableUtils.GetQueryableOnlyQueryException(this.GetType(), "Query");
        }

        public override Task<SingleResult<TData>> LookupAsync(string id)
        {
            throw TableUtils.GetQueryableOnlyLookupException(this.GetType(), "Lookup");
        }

        public override async Task<TData> InsertAsync(TData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Id == null)
            {
                data.Id = Guid.NewGuid().ToString("N");
            }

            TModel model = Mapper.Map<TData, TModel>(data);
            this.Context.Set<TModel>().Add(model);

            await this.SubmitChangesAsync();

            return Mapper.Map<TModel, TData>(model);
        }

        public virtual Task<TData> UpdateAsync(string id, Delta<TData> patch, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public override Task<TData> UndeleteAsync(string id, Delta<TData> patch)
        {
            patch = patch ?? new Delta<TData>();
            patch.TrySetPropertyValue(TableUtils.DeletedPropertyName, false);
            return this.UpdateAsync(id, patch, includeDeleted: true);
        }

        public override async Task<TData> ReplaceAsync(string id, TData data)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.VerifyUpdatedKey(id, data);
            TModel model = Mapper.Map<TData, TModel>(data);

            try
            {
                this.Context.Entry(model).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                string updateError = EFResources.DomainManager_InvalidOperation.FormatForUser(ex.Message);
                this.Request.GetConfiguration().Services.GetTraceWriter().Debug(updateError, this.Request, LogCategories.TableControllers);
                HttpResponseMessage invalid = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, updateError, ex);
                throw new HttpResponseException(invalid);
            }

            await this.SubmitChangesAsync();

            return Mapper.Map<TData>(model);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required to pass an Expression.")]
        protected virtual SingleResult<TData> LookupEntity(Expression<Func<TModel, bool>> filter)
        {
            return this.LookupEntity(filter, this.IncludeDeleted);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required to pass an Expression.")]
        protected virtual SingleResult<TData> LookupEntity(Expression<Func<TModel, bool>> filter, bool includeDeleted)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            IQueryable<TData> query = this.Context.Set<TModel>().Where(filter).ProjectTo<TData>();
            query = TableUtils.ApplyDeletedFilter(query, includeDeleted);
            return SingleResult.Create(query);
        }

        protected virtual Task<TData> UpdateEntityAsync(Delta<TData> patch, params object[] keys)
        {
            return this.UpdateEntityAsync(patch, this.IncludeDeleted, keys);
        }

        protected virtual async Task<TData> UpdateEntityAsync(Delta<TData> patch, bool includeDeleted, params object[] keys)
        {
            if (patch == null)
            {
                throw new ArgumentNullException("patch");
            }

            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            TModel model = await this.Context.Set<TModel>().FindAsync(keys);
            if (model == null)
            {
                throw new HttpResponseException(this.Request.CreateNotFoundResponse());
            }

            TData data = Mapper.Map<TModel, TData>(model);
            if (!includeDeleted && data.Deleted)
            {
                throw new HttpResponseException(this.Request.CreateNotFoundResponse());
            }

            // Set the original version based on etag (if present)
            byte[] patchVersion = patch.GetPropertyValueOrDefault<TData, byte[]>(TableUtils.VersionPropertyName);
            if (patchVersion != null)
            {
                this.SetOriginalVersion(model, patchVersion);
            }

            patch.Patch(data);
            Mapper.Map<TData, TModel>(data, model);

            await this.SubmitChangesAsync();

            return Mapper.Map<TData>(model);
        }

        protected virtual async Task<bool> DeleteItemAsync(params object[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            DbSet<TModel> set = this.Context.Set<TModel>();
            TModel model = await set.FindAsync(keys);
            if (model == null)
            {
                return false;
            }

            TData data = Mapper.Map<TModel, TData>(model);
            if (this.EnableSoftDelete && data.Deleted)
            {
                return false; // if soft delete is enabled then deleted record should return 404
            }

            // Set the original version based on etag (if present)
            byte[] version = this.Request.GetVersionFromIfMatch();
            if (version != null)
            {
                this.SetOriginalVersion(model, version);
            }

            if (this.EnableSoftDelete)
            {
                data.Deleted = true;
                Mapper.Map<TData, TModel>(data, model);
            }
            else
            {
                set.Remove(model);
            }

            int result = await this.SubmitChangesAsync();
            return result > 0;
        }

        /// <summary>
        /// Submits the change through Entity Framework while logging any exceptions
        /// and produce appropriate <see cref="HttpResponseMessage"/> instances.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        protected virtual Task<int> SubmitChangesAsync()
        {
            return EntityUtils.SubmitChangesAsync(this.Context, this.Request, this.GetOriginalValue);
        }

        /// <summary>
        /// Gets the original value of an entity in case an update or replace operation
        /// resulted in an <see cref="DbUpdateConcurrencyException"/>. The original value extracted
        /// from the exception will get returned to the client so that it can merge the data and
        /// possibly try the operation again.
        /// </summary>
        /// <param name="conflict">The <see cref="DbUpdateConcurrencyException"/> thrown by
        /// the update or replace operation.</param>
        /// <returns>The original value or <c>null</c> if none are available.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "keeping the derived class is better from a usability point of view.")]
        protected virtual object GetOriginalValue(DbUpdateConcurrencyException conflict)
        {
            DbEntityEntry entity = conflict != null && conflict.Entries != null ? conflict.Entries.FirstOrDefault() : null;
            if (entity == null)
            {
                return null;
            }

            DbPropertyValues values = entity.GetDatabaseValues();
            object model = values != null ? values.ToObject() : null;
            return model != null ? Mapper.Map<TData>(model) : null;
        }

        protected virtual TKey GetKey<TKey>(string id)
        {
            return this.GetKey<TKey>(id, CultureInfo.InvariantCulture);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by caller.")]
        protected virtual TKey GetKey<TKey>(string id, CultureInfo culture)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }

            try
            {
                return (TKey)((IConvertible)id).ToType(typeof(TKey), culture);
            }
            catch
            {
                string error = EFResources.EntityDomainController_KeyNotFound.FormatForUser(id, typeof(TKey).Name);
                throw new HttpResponseException(this.Request.CreateNotFoundResponse(error));
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by caller.")]
        protected virtual CompositeTableKey GetCompositeKey(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            CompositeTableKey compositeKey;
            if (CompositeTableKey.TryParse(id, out compositeKey))
            {
                return compositeKey;
            }

            string error = EFResources.TableKeys_InvalidKey.FormatForUser(id);
            throw new HttpResponseException(this.Request.CreateNotFoundResponse(error));
        }

        /// <summary>
        /// Override this method to support optimistic concurrent updates and replace operations. In order to evaluate whether
        /// an object has been updated concurrently, Entity Framework needs to know the original version of the object being edited
        /// and compare that to the current version maintained in the database. This requires that the original value is known
        /// by Entity Framework so that it can ask the database to do the check.
        /// </summary>
        /// <remarks>
        /// In case the column used to manage versioning is called "Version", the code to set the original value looks like this:
        /// <code>
        ///   this.context.Entry(model).OriginalValues["Version"] = version;
        /// </code>
        /// </remarks>
        /// <param name="model">The current entity model object.</param>
        /// <param name="version">The original version provided by the request as being the version that is being updated; or <c>null</c> if no version was indicated.</param>
        protected virtual void SetOriginalVersion(TModel model, byte[] version)
        {
            this.Request.GetConfiguration().Services.GetTraceWriter().Error(EFResources.DomainManager_NoOriginalValue.FormatForUser(this.GetType().Name, "SetOriginalVersion"), this.Request, LogCategories.TableControllers);
        }

        /// <summary>
        /// Verify that the id specified in the request URI matches that of an
        /// updated entity. If not then throw an exception as ids can't be modified.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns response object.")]
        private void VerifyUpdatedKey(string id, TData data)
        {
            // Verify that keys match after patch
            if (data == null || data.Id != id)
            {
                string msg = EFResources.TableController_KeyMismatch.FormatForUser("Id", id, data.Id);
                HttpResponseMessage badKey = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(badKey);
            }
        }
    }
}