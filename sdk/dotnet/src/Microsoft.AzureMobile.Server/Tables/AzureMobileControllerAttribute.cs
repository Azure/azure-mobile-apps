// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AzureMobile.Server.Extensions;

namespace Microsoft.AzureMobile.Server.Tables
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

            base.OnResultExecuting(context);
        }
    }
}
