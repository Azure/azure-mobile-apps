// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AzureMobile.Server.Exceptions;
using Microsoft.AzureMobile.Server.Extensions;

namespace Microsoft.AzureMobile.Server.Filters
{
    /// <summary>
    /// When the main table controller throws an exception, it is caught and turned
    /// into an appropriate HTTP response.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class AzureMobileExceptionsAttribute : ExceptionFilterAttribute
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
