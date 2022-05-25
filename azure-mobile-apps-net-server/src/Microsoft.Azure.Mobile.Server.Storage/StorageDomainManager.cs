// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// Provides an <see cref="DomainManager{T}"/> implementation targeting Azure Table Storage as the backend store.
    /// </summary>
    /// <typeparam name="TData">The data object (DTO) type.</typeparam>
    [CLSCompliant(false)]
    public class StorageDomainManager<TData> : DomainManager<TData>
        where TData : class, ITableData, ITableEntity, new()
    {
        private const string ModelKeyPrefix = "MS_EdmModel";
        private const string QuerySeparator = "&";
        private const string NextPartitionKeyParamName = "NextPartitionKey";
        private const string NextRowKeyParamName = "NextRowKey";
        private const string NextTableNameParamName = "NextTableName";
        private const string ODataTokenPrefix = "([(=,&0+])";
        private const string ODataTokenSuffix = "([)=,&%+]|$)";
        private const string DateTimeOffsetReplaceString = "$1datetime%27$2%27$3";

        private static readonly Regex DateTimeOffsetPattern = new Regex(ODataTokenPrefix + // some odata token before
                                                                        @"datetimeoffset(?:%27|')(" + // the token: datetimeoffset'(
                                                                        @"\d{4}-\d{2}-\d{2}" + // date: 1970-01-01
                                                                        @"T\d{2}%3A\d{2}%3A\d{2}" + // time: T00:00:00
                                                                        @"(?:\.\d{3,7}){0,1}" + // fraction: (?: .3255367{0,1})
                                                                        @"(?:Z|(?:-|%2B)\d{2}%3A\d{2})" + // zone: (?:Z|(-|+)00:00)
                                                                        @")(?:%27|')" + // the end quote: )'
                                                                        ODataTokenSuffix, RegexOptions.Compiled);  // some odata token after

        private static readonly IDictionary<string, string> StorageDataSystemPropertyMap = new Dictionary<string, string>()
        {
            { "updatedAt", "Timestamp" },
            { "version", "ETag" }
        };

        private static readonly string ExcludeDeletedFilter = String.Format(CultureInfo.InvariantCulture, "{0} ne true", TableUtils.DeletedPropertyName);
        private static readonly ConcurrentDictionary<string, CloudStorageAccount> CloudStorageAccounts = new ConcurrentDictionary<string, CloudStorageAccount>();
        private static readonly ODataValidationSettings DefaultValidationSettings = GetDefaultValidationSettings();
        private static readonly ODataQuerySettings DefaultQuerySetttings = GetDefaultQuerySettings();

        private ODataValidationSettings validationSettings;
        private ODataQuerySettings querySettings;
        private IDictionary<string, string> systemPropertyMap;

        /// <summary>
        /// Creates a new instance of <see cref="StorageDomainManager{TData}"/>
        /// </summary>
        /// <param name="connectionStringName">
        /// Name of the storage connection string
        /// </param>
        /// <param name="tableName">
        /// Name of table
        /// </param>
        /// <param name="request">
        /// An instance of <see cref="HttpRequestMessage"/>
        /// </param>
        public StorageDomainManager(string connectionStringName, string tableName, HttpRequestMessage request)
            : this(connectionStringName, tableName, request, DefaultValidationSettings, DefaultQuerySetttings)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="StorageDomainManager{TData}"/>
        /// </summary>
        /// <param name="connectionStringName">
        /// Name of the storage connection string
        /// </param>
        /// <param name="tableName">
        /// Name of table
        /// </param>
        /// <param name="request">
        /// An instance of <see cref="HttpRequestMessage"/>
        /// </param>
        /// <param name="enableSoftDelete">
        /// Determines whether rows are hard deleted or marked as deleted.
        /// </param>
        public StorageDomainManager(string connectionStringName, string tableName, HttpRequestMessage request, bool enableSoftDelete)
            : this(connectionStringName, tableName, request, DefaultValidationSettings, DefaultQuerySetttings, enableSoftDelete)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="StorageDomainManager{TData}"/>
        /// </summary>
        /// <param name="connectionStringName">
        /// Name of the storage connection string
        /// </param>
        /// <param name="tableName">
        /// Name of table
        /// </param>
        /// <param name="request">
        /// An instance of <see cref="HttpRequestMessage"/>
        /// </param>
        /// <param name="querySettings">
        /// An instance of <see cref="ODataQuerySettings"/>
        /// </param>
        /// <param name="validationSettings">
        /// An instance of <see cref="ODataValidationSettings"/>
        /// </param>
        public StorageDomainManager(string connectionStringName, string tableName, HttpRequestMessage request, ODataValidationSettings validationSettings, ODataQuerySettings querySettings)
            : this(connectionStringName, tableName, request, validationSettings, querySettings, enableSoftDelete: false)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="StorageDomainManager{TData}"/>
        /// </summary>
        /// <param name="connectionStringName">
        /// Name of the storage connection string
        /// </param>
        /// <param name="tableName">
        /// Name of table
        /// </param>
        /// <param name="request">
        /// An instance of <see cref="HttpRequestMessage"/>
        /// </param>
        /// <param name="querySettings">
        /// An instance of <see cref="ODataQuerySettings"/>
        /// </param>
        /// <param name="validationSettings">
        /// An instance of <see cref="ODataValidationSettings"/>
        /// </param>
        /// <param name="enableSoftDelete">
        /// Determines whether rows are hard deleted or marked as deleted.
        /// </param>
        public StorageDomainManager(string connectionStringName, string tableName, HttpRequestMessage request, ODataValidationSettings validationSettings, ODataQuerySettings querySettings, bool enableSoftDelete)
            : base(request, enableSoftDelete)
        {
            if (connectionStringName == null)
            {
                throw new ArgumentNullException("connectionStringName");
            }

            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (validationSettings == null)
            {
                throw new ArgumentNullException("validationSettings");
            }

            if (querySettings == null)
            {
                throw new ArgumentNullException("querySettings");
            }

            this.validationSettings = validationSettings;
            this.querySettings = querySettings;

            this.StorageAccount = CloudStorageAccounts.GetOrAdd(connectionStringName, this.GetCloudStorageAccount);
            this.TableName = tableName;
            this.TableClient = this.StorageAccount.CreateCloudTableClient();
            this.Table = this.TableClient.GetTableReference(this.TableName);
            this.systemPropertyMap = typeof(StorageData).IsAssignableFrom(typeof(TData)) ? StorageDataSystemPropertyMap : null;
        }

        internal enum OperationType
        {
            Insert,
            Delete,
            Replace,
            Read,
            Undelete,
            Update
        }

        public ODataValidationSettings ValidationSettings
        {
            get
            {
                return this.validationSettings;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.validationSettings = value;
            }
        }

        public ODataQuerySettings QuerySettings
        {
            get
            {
                return this.querySettings;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.querySettings = value;
            }
        }

        protected CloudStorageAccount StorageAccount { get; set; }

        protected CloudTableClient TableClient { get; set; }

        protected string TableName { get; set; }

        protected CloudTable Table { get; set; }

        public async override Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            var operation = new TableQuery<TData>();

            // Set selected properties but remove 'ETag' which is not an actual column
            bool isSelectModified;
            IList<string> properties = this.Request.SetSelectedProperties(typeof(TData), this.systemPropertyMap, out isSelectModified);
            properties.Remove("ETag");
            operation.SelectColumns = properties;

            var context = new ODataQueryContext(this.GetEdmModel(), typeof(TData));
            query = new ODataQueryOptions(context, this.Request);

            try
            {
                query.Validate(this.validationSettings);
            }
            catch (ODataException e)
            {
                HttpResponseMessage badQuery = this.Request.CreateBadRequestResponse(ASResources.DomainManager_InvalidQueryUri.FormatForUser(e.Message));
                throw new HttpResponseException(badQuery);
            }

            // Apply any filter provided to the query
            if (query.Filter != null && !string.IsNullOrEmpty(query.Filter.RawValue))
            {
                string filterString = ReplaceDateTimeOffsetWithDateTime(query.Filter.RawValue);
                if (this.IncludeDeleted)
                {
                    operation.FilterString = filterString;
                }
                else
                {
                    operation.FilterString = String.Format(CultureInfo.InvariantCulture, "({0}) and ({1})", filterString, ExcludeDeletedFilter);
                }
            }
            else if (!this.IncludeDeleted)
            {
                operation.FilterString = ExcludeDeletedFilter;
            }

            // Apply the max result size for this query
            int resultSize = TableUtils.GetResultSize(query, this.querySettings);
            operation.TakeCount = resultSize;

            return await this.ExecuteQueryAsync(operation, resultSize);
        }

        internal static string ReplaceDateTimeOffsetWithDateTime(string filterString)
        {
            filterString = Uri.EscapeDataString(filterString);
            filterString = DateTimeOffsetPattern.Replace(filterString, DateTimeOffsetReplaceString);
            filterString = Uri.UnescapeDataString(filterString);
            return filterString;
        }

        private IEdmModel GetEdmModel()
        {
            var actionDescriptor = this.Request.GetActionDescriptor();
            IEdmModel model = actionDescriptor == null ? null : (IEdmModel)actionDescriptor.Properties[ModelKeyPrefix + typeof(TData).FullName];
            if (model != null)
            {
                return model;
            }

            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<TData>(this.TableName);
            model = builder.GetEdmModel();

            return model;
        }

        public override async Task<SingleResult<TData>> LookupAsync(string id)
        {
            TData data = await this.LookupItemAsync(id);
            return SingleResult<TData>.Create(new TData[] { data }.AsQueryable<TData>());
        }

        private async Task<TData> LookupItemAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            CompositeTableKey key = this.GetStorageKey(id);
            TData data = await this.GetCurrentItem(key, this.IncludeDeleted);
            return data;
        }

        public override IQueryable<TData> Query()
        {
            throw TableUtils.GetNoQueryableQueryException(this.GetType(), "QueryAsync");
        }

        public override SingleResult<TData> Lookup(string id)
        {
            throw TableUtils.GetNoQueryableLookupException(this.GetType(), "LookupAsync");
        }

        public override async Task<TData> InsertAsync(TData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            data.CreatedAt = DateTimeOffset.UtcNow;

            TableOperation insertOperation = TableOperation.Insert(data);
            return await this.ExecuteOperationAsync(insertOperation, OperationType.Insert);
        }

        public override Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            return this.UpdateAsync(id, patch, this.IncludeDeleted, OperationType.Update);
        }

        public override Task<TData> UndeleteAsync(string id, Delta<TData> patch)
        {
            patch = patch ?? new Delta<TData>();
            patch.TrySetPropertyValue(TableUtils.DeletedPropertyName, false);
            return this.UpdateAsync(id, patch, true, OperationType.Undelete);
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

            CompositeTableKey key = this.GetStorageKey(id);
            this.VerifyUpdatedKey(key, data);
            data.CreatedAt = DateTimeOffset.UtcNow;

            TableOperation replace = TableOperation.Replace(data);
            return await this.ExecuteOperationAsync(replace, OperationType.Replace);
        }

        public override async Task<bool> DeleteAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            CompositeTableKey key = this.GetStorageKey(id);

            TableOperation operation = null;
            TData data = await this.GetCurrentItem(key, includeDeleted: !this.IncludeDeleted);
            if (this.EnableSoftDelete)
            {
                data.Deleted = true;
                operation = TableOperation.Merge(data);
            }
            else
            {
                operation = TableOperation.Delete(data);
            }

            bool deleted = false;
            if (operation != null)
            {
                await this.ExecuteOperationAsync(operation, OperationType.Delete);
                deleted = true;
            }

            return deleted;
        }

        protected virtual CloudStorageAccount GetCloudStorageAccount(string connectionStringName)
        {
            IMobileAppSettingsProvider provider = this.Request.GetConfiguration().GetMobileAppSettingsProvider();
            ConnectionSettings connectionSettings;
            if (!provider.GetMobileAppSettings().Connections.TryGetValue(connectionStringName, out connectionSettings))
            {
                throw new ArgumentException(ASResources.DomainManager_ConnectionStringNotFound.FormatForUser(connectionStringName));
            }

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionSettings.ConnectionString);
            if (cloudStorageAccount == null)
            {
                throw new ArgumentException(ASResources.StorageTable_NoCloudStorageAccount.FormatForUser(typeof(CloudStorageAccount).Name, connectionStringName));
            }

            return cloudStorageAccount;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by caller.")]
        protected virtual CompositeTableKey GetStorageKey(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            CompositeTableKey compositeKey;
            if (!CompositeTableKey.TryParse(id, out compositeKey) || compositeKey.Segments.Count != 2)
            {
                // We have either invalid, no keys, or too many keys
                string error = ASResources.StorageTable_InvalidKey.FormatForUser(id);
                throw new HttpResponseException(this.Request.CreateBadRequestResponse(error));
            }

            return compositeKey;
        }

        private async Task<TData> UpdateAsync(string id, Delta<TData> patch, bool includeDeleted, OperationType operationType)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (patch == null)
            {
                throw new ArgumentNullException("patch");
            }

            CompositeTableKey key = this.GetStorageKey(id);
            TData current = await this.GetCurrentItem(key, includeDeleted);
            patch.Patch(current);
            this.VerifyUpdatedKey(key, current);

            TableOperation update = TableOperation.Merge(current);
            return await this.ExecuteOperationAsync(update, operationType, key);
        }

        protected virtual Task<TData> GetCurrentItem(CompositeTableKey key)
        {
            return this.GetCurrentItem(key, this.IncludeDeleted);
        }

        private async Task<TData> GetCurrentItem(CompositeTableKey key, bool includeDeleted)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            TableOperation query = TableOperation.Retrieve<TData>(key.Segments[0], key.Segments[1]);
            TData item = await this.ExecuteOperationAsync(query, OperationType.Read);
            if (item == null || (!includeDeleted && item.Deleted))
            {
                throw new HttpResponseException(this.Request.CreateNotFoundResponse());
            }

            return item;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by caller.")]
        internal async Task<HttpResponseException> ConvertStorageException(StorageException exception, CompositeTableKey key)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            ITraceWriter traceWriter = this.Request.GetConfiguration().Services.GetTraceWriter();
            HttpResponseMessage error = null;

            if ((exception.RequestInformation != null) && (exception.RequestInformation.ExtendedErrorInformation != null))
            {
                string message = CommonResources.DomainManager_Conflict.FormatForUser(exception.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                traceWriter.Info(message, this.Request, LogCategories.TableControllers);

                if (exception.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.EntityAlreadyExists)
                {
                    error = this.Request.CreateErrorResponse(HttpStatusCode.Conflict, message);
                }
                else if (exception.RequestInformation.ExtendedErrorInformation.ErrorCode == StorageErrorCodeStrings.ConditionNotMet ||
                         exception.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.UpdateConditionNotSatisfied)
                {
                    // the two values above indicate that the etag doesn't match, but the error thrown from the emulator 
                    // is different from that in the service, so both must be included

                    var content = await this.GetCurrentItem(key);
                    error = this.Request.CreateResponse(HttpStatusCode.PreconditionFailed, content);
                }
                else if (exception.RequestInformation.ExtendedErrorInformation.ErrorCode == StorageErrorCodeStrings.ResourceNotFound)
                {
                    error = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                }
            }

            if (error == null)
            {
                string message = ASResources.DomainManager_InvalidOperation.FormatForUser(exception.Message);
                error = this.Request.CreateBadRequestResponse(message);
                traceWriter.Error(message, this.Request, LogCategories.TableControllers);
            }

            return new HttpResponseException(error);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by caller.")]
        private async Task<TData> ExecuteOperationAsync(TableOperation operation, OperationType operationType, CompositeTableKey key = null, bool createTable = false)
        {
            StorageException storageException = null;

            try
            {
                if (createTable)
                {
                    await this.Table.CreateIfNotExistsAsync();
                }

                var tableResult = await this.Table.ExecuteAsync(operation);
                return tableResult.Result as TData;
            }
            catch (StorageException exception)
            {
                storageException = exception;
            }

            if (TableResponseNotFound(storageException) && !createTable)
            {
                return await this.ExecuteOperationAsync(operation, operationType, key, createTable: true);
            }
            else
            {
                throw await this.ConvertStorageException(storageException, key);
            }
        }

        protected virtual Task<IEnumerable<TData>> ExecuteQueryAsync(TableQuery<TData> query, int resultSize)
        {
            const int MinResultSize = 1;

            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if (resultSize < MinResultSize)
            {
                throw new ArgumentOutOfRangeException("resultSize", resultSize, CommonResources.ArgMustBeGreaterThanOrEqualTo.FormatForUser(MinResultSize));
            }

            return this.ExecuteQueryAsync(query, resultSize, createTable: false);
        }

        private async Task<IEnumerable<TData>> ExecuteQueryAsync(TableQuery<TData> query, int resultSize, bool createTable)
        {
            StorageException storageException = null;

            try
            {
                if (createTable)
                {
                    await this.Table.CreateIfNotExistsAsync();
                }

                // if there is a continuation specified on the request
                // we'll start from there
                TableContinuationToken continuationToken = GetContinuationToken(this.Request);

                TableQuerySegment<TData> segment = null;
                List<TData> result = new List<TData>();
                do
                {
                    segment = await this.Table.ExecuteQuerySegmentedAsync<TData>(query, continuationToken);
                    if (segment == null)
                    {
                        continuationToken = null;
                        break;
                    }

                    result.AddRange(segment);
                    continuationToken = segment.ContinuationToken;
                    if (result.Count >= resultSize)
                    {
                        break;
                    }
                }
                while (continuationToken != null);

                // If we have a continuation token then set a next link.
                this.SetNextPageLink(continuationToken);

                return result;
            }
            catch (StorageException exception)
            {
                storageException = exception;
            }

            if (TableResponseNotFound(storageException) && !createTable)
            {
                return await this.ExecuteQueryAsync(query, resultSize, createTable: true);
            }
            else
            {
                throw storageException;
            }
        }

        protected virtual void SetNextPageLink(TableContinuationToken continuationToken)
        {
            if (continuationToken == null)
            {
                return;
            }

            // first extract the query parameters and set (or update)
            // the continuation token values
            Dictionary<string, string> queryParameters = this.Request.GetQueryNameValuePairs().ToDictionary(p => p.Key, p => p.Value);
            queryParameters[NextPartitionKeyParamName] = continuationToken.NextPartitionKey;
            queryParameters[NextRowKeyParamName] = continuationToken.NextRowKey;
            queryParameters[NextTableNameParamName] = continuationToken.NextTableName;

            // build the link uri by starting with the initial base uri and replacing
            // the query portiion with the newly constructed parameters above
            UriBuilder uriBuilder = new UriBuilder(this.Request.RequestUri);
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var parameter in queryParameters)
            {
                first = AppendQueryParameter(sb, parameter.Key, parameter.Value, first);
            }

            uriBuilder.Query = sb.ToString();

            this.Request.ODataProperties().NextLink = uriBuilder.Uri;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller owns response object.")]
        private void VerifyUpdatedKey(CompositeTableKey key, TData data)
        {
            if (data == null || data.PartitionKey != key.Segments[0] || data.RowKey != key.Segments[1])
            {
                string msg = ASResources.TableController_KeyMismatch.FormatForUser("Id", key.ToString(), data.Id);
                HttpResponseMessage badKey = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(badKey);
            }
        }

        internal static TableContinuationToken GetContinuationToken(HttpRequestMessage request)
        {
            TableContinuationToken continuationToken = null;
            Dictionary<string, string> queryParams = request.GetQueryNameValuePairs().ToDictionary(p => p.Key, p => p.Value);

            if (queryParams.ContainsKey(NextPartitionKeyParamName))
            {
                continuationToken = continuationToken ?? new TableContinuationToken();
                continuationToken.NextPartitionKey = queryParams[NextPartitionKeyParamName];
            }

            if (queryParams.ContainsKey(NextRowKeyParamName))
            {
                continuationToken = continuationToken ?? new TableContinuationToken();
                continuationToken.NextRowKey = queryParams[NextRowKeyParamName];
            }

            if (queryParams.ContainsKey(NextTableNameParamName))
            {
                continuationToken = continuationToken ?? new TableContinuationToken();
                continuationToken.NextTableName = queryParams[NextTableNameParamName];
            }

            return continuationToken;
        }

        internal static bool TableResponseNotFound(StorageException exception)
        {
            var requestInfo = exception.RequestInformation;
            return (requestInfo != null) && (requestInfo.HttpStatusCode == 404);
        }

        internal static ODataValidationSettings GetDefaultValidationSettings()
        {
            return new ODataValidationSettings()
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,

                AllowedFunctions = AllowedFunctions.None,

                AllowedQueryOptions = AllowedQueryOptions.Filter
                    | AllowedQueryOptions.Top
                    | AllowedQueryOptions.Select,

                AllowedLogicalOperators = AllowedLogicalOperators.Equal
                    | AllowedLogicalOperators.And
                    | AllowedLogicalOperators.Or
                    | AllowedLogicalOperators.Not
                    | AllowedLogicalOperators.GreaterThan
                    | AllowedLogicalOperators.GreaterThanOrEqual
                    | AllowedLogicalOperators.LessThan
                    | AllowedLogicalOperators.LessThanOrEqual
                    | AllowedLogicalOperators.NotEqual
            };
        }

        internal static ODataQuerySettings GetDefaultQuerySettings()
        {
            return new ODataQuerySettings()
            {
                PageSize = TableUtils.PageSize
            };
        }

        private static bool AppendQueryParameter(StringBuilder builder, string name, string value, bool first)
        {
            if (!String.IsNullOrEmpty(value))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", first ? string.Empty : QuerySeparator, name, value);
                first = false;
            }

            return first;
        }
    }
}