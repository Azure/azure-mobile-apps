// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.AspNetCore.Datasync.Filters
{
    /// <summary>
    /// When the main table controller throws an exception, it is caught and turned
    /// into an appropriate HTTP response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class DatasyncExceptionsAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is HttpException httpException)
            {
                context.Result = httpException.ToActionResult();
                if (httpException.Payload is ITableData entity)
                {
                    context.HttpContext.Response.Headers.AddFromEntity(entity);
                }
            }
        }
    }
}
