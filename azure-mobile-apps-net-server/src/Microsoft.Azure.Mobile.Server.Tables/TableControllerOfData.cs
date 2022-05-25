// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// Provides a common <see cref="ApiController"/> abstraction for Table Controllers.
    /// </summary>
    /// <typeparam name="TData">The type of the entity.</typeparam>
    public abstract class TableController<TData> : TableController
        where TData : class, ITableData
    {
        private IDomainManager<TData> domainManager;
        private ITraceWriter traceWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableController{TData}"/> class.
        /// </summary>
        protected TableController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableController{TData}"/> class
        /// with a given <paramref name="domainManager"/>.
        /// </summary>
        /// <param name="domainManager">The <see cref="IDomainManager{T}"/> for this controller.</param>
        protected TableController(IDomainManager<TData> domainManager)
        {
            if (domainManager == null)
            {
                throw new ArgumentNullException("domainManager");
            }

            this.DomainManager = domainManager;
        }

        /// <summary>
        /// Gets or sets the <see cref="IDomainManager{TData}"/> to be used for accessing the backend store.
        /// </summary>
        protected IDomainManager<TData> DomainManager
        {
            get
            {
                if (this.domainManager == null)
                {
                    string msg = TResources.TableController_NoDomainManager.FormatForUser("DomainManager", typeof(IDomainManager<>).GetShortName());
                    throw new InvalidOperationException(msg);
                }

                return this.domainManager;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.domainManager = value;
            }
        }

        /// <inheritdoc />
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.traceWriter = this.Configuration.Services.GetTraceWriter();
        }

        /// <summary>
        /// Provides a helper method for querying a backend store. It deals with any exceptions thrown by the <see cref="IDomainManager{TData}"/>
        /// and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>An <see cref="IQueryable{TData}"/> returned by the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller disposes response.")]
        protected virtual IQueryable<TData> Query()
        {
            try
            {
                return this.DomainManager.Query();
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// Provides a helper method for querying a backend store. It deals with any exceptions thrown by the <see cref="IDomainManager{TData}"/>
        /// and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>An <see cref="IQueryable{TData}"/> returned by the <see cref="IDomainManager{TData}"/>.</returns>
        protected virtual Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query)
        {
            return this.DomainManager.QueryAsync(query);
        }

        /// <summary>
        /// Provides a helper method for looking up an entity in a backend store. It deals with any exceptions thrown by the <see cref="IDomainManager{TData}"/>
        /// and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>An <see cref="SingleResult{TData}"/> returned by the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller disposes response.")]
        protected virtual SingleResult<TData> Lookup(string id)
        {
            try
            {
                return this.DomainManager.Lookup(id);
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// Provides a helper method for looking up an entity in a backend store. It deals with any exceptions thrown by the <see cref="IDomainManager{TData}"/>
        /// and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>An <see cref="SingleResult{TData}"/> returned by the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller disposes response.")]
        protected virtual async Task<SingleResult<TData>> LookupAsync(string id)
        {
            try
            {
                return await this.DomainManager.LookupAsync(id);
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// Provides a helper method for inserting an entity into a backend store. It deals with any model validation errors as well as
        /// exceptions thrown by the <see cref="IDomainManager{TData}"/> and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>A <see cref="Task{TData}"/> representing the insert operation executed by the the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed in the response path.")]
        protected async virtual Task<TData> InsertAsync(TData item)
        {
            if (item == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, TResources.TableController_NullRequestBody));
            }

            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            try
            {
                return await this.DomainManager.InsertAsync(item);
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// Provides a helper method for updating an entity in a backend store. It deals with any model validation errors as well as
        /// exceptions thrown by the <see cref="IDomainManager{TData}"/> and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>A <see cref="Task{TData}"/> representing the update operation executed by the the <see cref="IDomainManager{TData}"/>.</returns>
        protected virtual Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            return this.PatchAsync(id, patch, (i, p) => this.DomainManager.UpdateAsync(i, p));
        }

        /// <summary>
        /// Provides a helper method for undeleting an entity in a backend store. It deals with any model validation errors as well as
        /// exceptions thrown by the <see cref="IDomainManager{TData}"/> and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>A <see cref="Task{TData}"/> representing the update operation executed by the the <see cref="IDomainManager{TData}"/>.</returns>
        protected virtual Task<TData> UndeleteAsync(string id)
        {
            return this.UndeleteAsync(id, new Delta<TData>());
        }

        /// <summary>
        /// Provides a helper method for undeleting an entity in a backend store. It deals with any model validation errors as well as
        /// exceptions thrown by the <see cref="IDomainManager{TData}"/> and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>A <see cref="Task{TData}"/> representing the undelete operation executed by the the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed in the response path.")]
        protected virtual Task<TData> UndeleteAsync(string id, Delta<TData> patch)
        {
            var manager = this.DomainManager as DomainManager<TData>;
            if (manager == null)
            {
                throw new NotSupportedException(TResources.DomainManager_DoesNotSupportSoftDelete);
            }

            if (patch == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, TResources.TableController_NullRequestBody));
            }

            return this.PatchAsync(id, patch, (i, p) => manager.UndeleteAsync(i, p));
        }

        /// <summary>
        /// Provides a helper method for replacing an entity in a backend store. It deals with any model validation errors as well as
        /// exceptions thrown by the <see cref="IDomainManager{TData}"/> and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>A <see cref="Task{TData}"/> representing the replace operation executed by the the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed in the response path.")]
        protected async virtual Task<TData> ReplaceAsync(string id, TData item)
        {
            if (item == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, TResources.TableController_NullRequestBody));
            }

            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            // Add ETag from request If-Match header
            byte[] version = this.Request.GetVersionFromIfMatch();
            if (version != null)
            {
                item.Version = version;
            }

            try
            {
                return await this.DomainManager.ReplaceAsync(id, item);
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// Provides a helper method for deleting an entity from a backend store. It deals with any
        /// exceptions thrown by the <see cref="IDomainManager{TData}"/> and maps them into appropriate HTTP responses.
        /// </summary>
        /// <returns>A <see cref="Task{TData}"/> representing the delete operation executed by the the <see cref="IDomainManager{TData}"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed in the response path.")]
        protected virtual async Task DeleteAsync(string id)
        {
            bool result = false;
            try
            {
                result = await this.DomainManager.DeleteAsync(id);
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            if (!result)
            {
                throw new HttpResponseException(this.Request.CreateResponse(HttpStatusCode.NotFound));
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed in the response path.")]
        private async Task<TData> PatchAsync(string id, Delta<TData> patch, Func<string, Delta<TData>, Task<TData>> patchAction)
        {
            if (patch == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, TResources.TableController_NullRequestBody));
            }

            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            // Add ETag from request If-Match header
            byte[] version = this.Request.GetVersionFromIfMatch();
            if (version != null)
            {
                if (!patch.TrySetPropertyValue(TableUtils.VersionPropertyName, version))
                {
                    string error = TResources.TableController_CouldNotSetVersion.FormatForUser(TableUtils.VersionPropertyName, version);
                    this.traceWriter.Error(error, this.Request, ServiceLogCategories.TableControllers);
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error));
                }
            }

            try
            {
                return await patchAction(id, patch);
            }
            catch (HttpResponseException ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw;
            }
            catch (Exception ex)
            {
                this.traceWriter.Error(ex, this.Request, LogCategories.TableControllers);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }
    }
}