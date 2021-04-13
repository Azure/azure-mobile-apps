// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AzureMobile.Server.Extensions;
using Microsoft.AzureMobile.Server.Models;
using Microsoft.AzureMobile.Server.Tables;

namespace Microsoft.AzureMobile.Server.Filters
{
    /// <summary>
    /// Adds the required headers to any response that sends a single entity to the client.
    /// This includes the ETag and Last-Modified headers currently.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class AzureMobileControllerAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult result && result.Value is ITableData entity)
            {
                context.HttpContext.Response.Headers.AddFromEntity(entity);
            }

            if (context.HttpContext.Request.IsZumoVersion(ZumoVersion.V2) && context.Result is ObjectResult objectResult && objectResult.Value is PagedResult pagedResult)
            {
                if (pagedResult.Count == null)
                {
                    context.Result = new OkObjectResult(pagedResult.Items);
                }
                else
                {
                    context.Result = new OkObjectResult(new Dictionary<string, object>()
                    {
                        { "results", pagedResult.Items },
                        { "count", pagedResult.Count }
                    });
                }
            }

            base.OnResultExecuting(context);
        }
    }
}
