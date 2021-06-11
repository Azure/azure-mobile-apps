// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.AspNetCore.Datasync.Filters
{
    /// <summary>
    /// Executes the resource filter before the controller.  Translates a v2 request before calling other
    /// middleware.
    /// </summary>
    /// <param name="resourceContext">The context for the request.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class AzureMobileBackwardsCompatibilityFilter : Attribute, IResourceFilter
    {
        private const string dateTimePattern = "datetime'([^']+)'";
        private const string dateTimeOffsetPattern = "datetimeoffset'([^']+)'";

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context.HttpContext.Request.IsProtocolVersion(ProtocolVersion.V2))
            {
                TranslateZumoV2Request(context);
            }
        }

        /// <summary>
        /// Executes the resource filter after the controller.  Translates a v2 response
        /// before calling other middleware.
        /// </summary>
        /// <param name="resourceContext">The context for the response.</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // Does nothing!
        }

        /// <summary>
        /// <para>The ZUMO-API-VERSION v2.0.0 request is different in several ways due to OData v3 vs. v4 changes.</para>
        /// <para>
        /// * <c>$inlinecount={allpages|none}</c> becomes <c>$count={true|false}</c>
        /// * <c>$filter</c> has date/time casts and contains vs substringof
        /// </para>
        /// </summary>
        /// <param name="context">The context for the resource filter</param>
        internal static void TranslateZumoV2Request(ResourceExecutingContext context)
        {
            var request = context.HttpContext.Request;
            if (request.Method != HttpMethods.Get || !request.QueryString.HasValue)
            {
                // We only need to adjust GET requests that have a query string.
                return;
            }

            NameValueCollection requestQuery = HttpUtility.ParseQueryString(request.QueryString.Value);
            if (request.Query.ContainsKey("$inlinecount"))
            {
                if (request.Query["$inlinecount"][0] == "allpages")
                    requestQuery.Add("$count", "true");
                requestQuery.Remove("$inlinecount");
            }
            if (request.Query.ContainsKey("$filter"))
            {
                requestQuery["$filter"] = TranslateV2Filter(request.Query["$filter"][0]);
            }

            request.QueryString = new QueryString($"?{requestQuery.ToString().TrimStart('?')}");
        }

        /// <summary>
        /// <para>Translate an OData v3 filter into an OData v4 filter.</para>
        /// <para>
        /// - <c>datetime'ISO'</c> becomes <c>cast('ISO',Edm.DateTime)</c>
        /// - <c>datetimeoffset'ISO'</c> becomes <c>cast('ISO',Edm.DateTimeOffset)</c>
        /// - <c>substringof(a,b)</c> becomes <c>contains(b,a)</c>
        /// </para>
        /// </summary>
        /// <param name="originalFilter"></param>
        /// <returns></returns>
        internal static string TranslateV2Filter(string originalFilter)
        {
            string replacedFilter = originalFilter;

            // datetime'ISO' replacement to cast('ISO',Edm.DateTime)
            replacedFilter = Regex.Replace(replacedFilter, dateTimePattern, "cast($1,Edm.DateTimeOffset)");

            // datetimeoffset'ISO' replacement to case('ISO', Edm.DateTimeOffset)
            replacedFilter = Regex.Replace(replacedFilter, dateTimeOffsetPattern, "cast($1,Edm.DateTimeOffset)");

            // substringof(a,b) replacement to contains(b,a)
            // TODO: substringof replacement

            return replacedFilter;
        }
    }
}
