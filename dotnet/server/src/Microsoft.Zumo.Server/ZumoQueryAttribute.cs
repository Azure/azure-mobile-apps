// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections;
using System.Linq;

namespace Microsoft.Zumo.Server
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ZumoQueryAttribute : EnableQueryAttribute
    {
        private const string CountParam = "$inlinecount";

        /// <summary>
        /// Called by the ASP.NET MVC framework after the action method executes.  The base
        /// <see cref="OnActionExecuted(ActionExecutedContext)"/> method will execute the
        /// OData query. Then, we check to see if the request includes $inlinecount=allpages
        /// and if it does, wrap in the count.
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException(nameof(actionExecutedContext));
            }

            // Determine if the query includes a request for the inline count.  Prior to calling
            // the base version, the Result contains a queryable with all elements that the user
            // can see in it.
            var request = actionExecutedContext.HttpContext.Request;
            bool countRequested = request.Query.ContainsKey(CountParam) && request.Query[CountParam].First().ToLowerInvariant() == "allpages";
            long inlineCount = -1;
            if (countRequested && IsSuccessStatusCode(actionExecutedContext.HttpContext.Response.StatusCode))
            {
                if (actionExecutedContext.Result is OkObjectResult okResult)
                {
                    if (okResult.Value is IQueryable<object> queryable)
                    {
                        inlineCount = queryable.LongCount();
                    }
                }
            }

            base.OnActionExecuted(actionExecutedContext);

            // Now the Result contains just the requested information.  We can augment this by providing
            // a count/results object
            if (countRequested && inlineCount >= 0 && IsSuccessStatusCode(actionExecutedContext.HttpContext.Response.StatusCode))
            {
                if (actionExecutedContext.Result is OkObjectResult odataResult)
                {
                    ZumoInlineCountResult inlineCountResult = new ZumoInlineCountResult
                    {
                        Results = odataResult.Value,
                        Count = inlineCount
                    };
                    actionExecutedContext.Result = new OkObjectResult(inlineCountResult);
                }
            }
        }

        /// <summary>
        /// Returns true if the Result within the context is successful.
        /// </summary>
        /// <param name="statusCode">The HTTP Status Code to check</param>
        /// <returns>true if successful</returns>
        internal bool IsSuccessStatusCode(int statusCode)
            => statusCode >= 200 && statusCode < 300;

        /// <summary>
        /// Replacement method for validating the query that also allows the $inlinecount
        /// query parameter.
        /// </summary>
        /// <param name="request">The HTTP Request</param>
        /// <param name="queryOptions">The OData Query Options</param>
        public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions));
            }

            foreach (var kvp in request.Query)
            {
                if (!IsSupportedQueryOption(queryOptions, kvp.Key) && kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    throw new ArgumentOutOfRangeException(kvp.Key);
                }
            }

            ODataValidationSettings validationSettings = BuildValidationSettings();
            queryOptions.Validate(validationSettings);
        }

        /// <summary>
        /// Builds the validation settings based on the known information within the current object.
        /// </summary>
        /// <returns>A <see cref="ODataValidationSettings"/> object</returns>
        private ODataValidationSettings BuildValidationSettings()
        {
            var result = new ODataValidationSettings
            {
                AllowedArithmeticOperators = this.AllowedArithmeticOperators,
                AllowedFunctions           = this.AllowedFunctions,
                AllowedLogicalOperators    = this.AllowedLogicalOperators,
                AllowedQueryOptions        = this.AllowedQueryOptions,
                MaxAnyAllExpressionDepth   = this.MaxAnyAllExpressionDepth,
                MaxNodeCount               = this.MaxNodeCount,
                MaxExpansionDepth          = this.MaxExpansionDepth,
                MaxOrderByNodeCount        = this.MaxOrderByNodeCount,
                MaxSkip                    = null,
                MaxTop                     = this.MaxTop,
            };
            return result;
        }

        /// <summary>
        /// Determines if the key is a supported system query option.
        /// </summary>
        /// <param name="options">The <see cref="ODataQueryOptions"/> object.</param>
        /// <param name="keyName">The name of the key.</param>
        /// <returns>True if the query option is supported.</returns>
        private bool IsSupportedQueryOption(ODataQueryOptions options, string keyName)
            => options.IsSupportedQueryOption(keyName) || keyName.ToLowerInvariant() == "$inlinecount";

        /// <summary>
        /// Provides the format for the object returned by the inline count.
        /// </summary>
        internal class ZumoInlineCountResult
        {
            public object Results { get; set; }
            public long? Count { get; set; }
        }
    }
}
