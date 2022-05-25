// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Filter for table controllers tailoring a query as follows:
    /// 1) Merges the $select query options with any system property selection options.
    /// 2) Adds support for <c>$inlinecount</c> by wrapping the response in a <see cref="QueryResult"/>.
    /// <remarks>
    /// For <see cref="IQueryable"/> returning actions marked with <see cref="QueryableAttribute"/>, this filter will 
    /// "rewrite" the request URL, merging any $select with the system property selection. This ensures that the selection
    /// is performed in the DB in cases where the <see cref="IQueryable"/> is backed by a DB. This minimizes the DB 
    /// response size.
    /// </remarks>
    /// </summary>
    internal sealed class TableQueryFilter : ActionFilterAttribute
    {
        private const string ResultElementTypeKey = "MS_ResultElementType";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            HttpRequestMessage request = actionContext.Request;
            if (request == null)
            {
                throw new InvalidOperationException(TResources.ParameterNull.FormatForUser("Request"));
            }

            if (request.IsQueryableAction(actionContext.ActionDescriptor))
            {
                ApplySystemProperties(request, actionContext.ActionDescriptor);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException("actionExecutedContext");
            }

            HttpRequestMessage request = actionExecutedContext.Request;
            if (request == null)
            {
                throw new InvalidOperationException(TResources.ParameterNull.FormatForUser("Request"));
            }

            HttpResponseMessage response = actionExecutedContext.Response;
            if (response == null)
            {
                return;
            }

            long? count = request.ODataProperties().TotalCount;
            IQueryable queryableResults = null;
            if (count.HasValue && response.TryGetContentValue(out queryableResults))
            {
                // Client has requested an inline count, so the actual response content will contain
                // the query results as well as the count. Create a new ObjectContent for the query results.
                QueryResult queryResult = new QueryResult(queryableResults, count);
                MediaTypeFormatter formatter = ((ObjectContent)response.Content).Formatter;
                response.Content = new ObjectContent<QueryResult>(queryResult, formatter);
            }

            AddETagResponseHeader(response);

            Uri nextAddr = request.ODataProperties().NextLink;
            if (nextAddr != null)
            {
                LinkHeaderValue nextLink = new LinkHeaderValue(nextAddr) { Rel = "next" };
                response.Headers.Add("Link", nextLink.ToString());
            }
        }

        internal static void AddETagResponseHeader(HttpResponseMessage response)
        {
            ObjectContent responseContent = response.Content as ObjectContent;
            if (responseContent != null && responseContent.Value != null)
            {
                ITableData tableData;
                ISelectExpandWrapper selectExpandWrapper;
                object value = responseContent.Value;

                if ((selectExpandWrapper = value as ISelectExpandWrapper) != null)
                {
                    IDictionary<string, object> properties = selectExpandWrapper.ToDictionary();
                    byte[] version = properties.GetValueOrDefault<byte[]>(TableUtils.VersionPropertyName);
                    if (version != null)
                    {
                        EntityTagHeaderValue etag = GetETagFromVersion(version);
                        response.Headers.ETag = etag;
                    }
                }

                if ((tableData = value as ITableData) != null)
                {
                    byte[] version = tableData.Version;
                    if (version != null)
                    {
                        EntityTagHeaderValue etag = GetETagFromVersion(version);
                        response.Headers.ETag = etag;
                    }
                }
            }
        }

        internal static EntityTagHeaderValue GetETagFromVersion(byte[] version)
        {
            string quotedEtag = HttpHeaderUtils.GetQuotedString(Convert.ToBase64String(version));
            return new EntityTagHeaderValue(quotedEtag);
        }

        internal static void ApplySystemProperties(HttpRequestMessage request, HttpActionDescriptor actionDescriptor)
        {
            Type elementType = actionDescriptor.Properties.GetOrAdd(ResultElementTypeKey, o => GetElementType(actionDescriptor)) as System.Type;
            if (elementType == null)
            {
                return;
            }

            bool isSelectModified;
            request.SetSelectedProperties(elementType, out isSelectModified);
        }

        internal static Type GetElementType(HttpActionDescriptor actionDescriptor)
        {
            Type returnType = actionDescriptor.ReturnType;
            if (returnType == typeof(HttpResponseMessage) || typeof(IHttpActionResult).IsAssignableFrom(actionDescriptor.ReturnType))
            {
                ResponseTypeAttribute responseType = actionDescriptor.GetCustomAttributes<ResponseTypeAttribute>(inherit: true).FirstOrDefault();
                if (responseType != null)
                {
                    returnType = responseType.ResponseType;
                }
            }

            if (returnType != null)
            {
                return returnType.GetEnumerableElementType();
            }

            return null;
        }
    }
}
