// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.Globalization;

namespace Microsoft.AspNetCore.Datasync.Filters;

/// <summary>
/// Adds the required headers to any response that sends a single entity to the client.
/// This includes the <c>ETag</c> and <c>Last-Modified</c> headers currently.  Also handles
/// any response required when exceptions are thrown.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DatasyncControllerAttribute : ResultFilterAttribute, IExceptionFilter
{
    /// <inheritdoc />
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult result)
        {
            if (result.Value is ITableData entity)
            {
                AddHeadersFromEntity(context.HttpContext.Response.Headers, entity);
                if (result.StatusCode == StatusCodes.Status201Created)
                {
                    context.HttpContext.Response.Headers.Location = $"{context.HttpContext.Request.GetDisplayUrl()}/{entity.Id}";
                }
            }
            IDatasyncServiceOptions options = GetDatasyncServiceOptions(context.HttpContext);
            context.Result = new JsonResult(result.Value, options.JsonSerializerOptions) { StatusCode = result.StatusCode };
        }
        base.OnResultExecuting(context);
    }

    /// <summary>
    /// Handles the <see cref="HttpException"/> set to write out the appropriate information.
    /// </summary>
    /// <param name="context">The <see cref="ExceptionContext"/> describing the filter context.</param>
    public void OnException(ExceptionContext context)
    {
        if (!context.ExceptionHandled && context.Exception is HttpException exception)
        {
            IDatasyncServiceOptions options = GetDatasyncServiceOptions(context.HttpContext);
            context.Result = exception.Payload != null
                ? new JsonResult(exception.Payload, options.JsonSerializerOptions) { StatusCode = exception.StatusCode }
                : new StatusCodeResult(exception.StatusCode);
            if (exception.Payload is ITableData entity)
            {
                AddHeadersFromEntity(context.HttpContext.Response.Headers, entity);
            }
            context.ExceptionHandled = true;
        }
    }

    /// <summary>
    /// Adds the required headers to the response, based on the provided entity.
    /// </summary>
    /// <param name="headers">The header dictionary to modify.</param>
    /// <param name="entity">The entity to use as basis for the changes.</param>
    internal static void AddHeadersFromEntity(IHeaderDictionary headers, ITableData entity)
    {
        headers.Remove(HeaderNames.ETag);
        headers.Remove(HeaderNames.LastModified);

        if (entity.Version.Length > 0)
        {
            headers.Append(HeaderNames.ETag, $"\"{Convert.ToBase64String(entity.Version)}\"");
        }

        if (entity.UpdatedAt.HasValue && entity.UpdatedAt.Value != default)
        {
            headers.Append(HeaderNames.LastModified, entity.UpdatedAt.Value.ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Retrieves the <see cref="IDatasyncServiceOptions"/> from the request services.
    /// </summary>
    /// <param name="context">The context to use to retrieve the settings.</param>
    /// <returns></returns>
    private static IDatasyncServiceOptions GetDatasyncServiceOptions(HttpContext context)
        => context.RequestServices?.GetService<IDatasyncServiceOptions>() ?? new DatasyncServiceOptions();
}
