// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Datasync.Tests.Filters;

[ExcludeFromCodeCoverage]
public class DatasyncControllerAttribute_Tests
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    public void AddHeadersFromEntity_RemovesEntityHeaders(bool includeETag, bool includeLastModified, bool setUpdatedAtToNull)
    {
        HeaderDictionary headers = new();
        if (includeETag) headers.Add("ETag", "\"foo\"");
        if (includeLastModified) headers.Add("Last-Modified", "Wed, 21 Oct 2015 07:28:00 GMT");
        ITableData entity = new TableData() { Version = Array.Empty<byte>(), UpdatedAt = setUpdatedAtToNull ? null : default(DateTimeOffset) };

        DatasyncControllerAttribute.AddHeadersFromEntity(headers, entity);

        headers.Should().NotContainKey("ETag");
        headers.Should().NotContainKey("Last-Modified");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddHeadersFromEntity_AddsETagHeader(bool includeHeader)
    {
        HeaderDictionary headers = new();
        if (includeHeader)
        {
            headers.Add("ETag", "\"foo\"");
        }
        ITableData entity = new TableData() { Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68 }, UpdatedAt = null };

        DatasyncControllerAttribute.AddHeadersFromEntity(headers, entity);

        headers.Should().ContainKey("ETag").WhoseValue.Should().ContainSingle(v => v == "\"YWJjZGVmZ2g=\"");
        headers.Should().NotContainKey("LastModified");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddHeadersFromEntity_AddLastModifiedHeader(bool includeHeader)
    {
        HeaderDictionary headers = new();
        if (includeHeader)
        {
            headers.Add("Last-Modified", "Wed, 21 Oct 2015 07:28:00 GMT");
        }
        ITableData entity = new TableData() { Version = Array.Empty<byte>(), UpdatedAt = DateTimeOffset.Parse("2023-11-13T13:30:05.1234Z") };

        DatasyncControllerAttribute.AddHeadersFromEntity(headers, entity);

        headers.Should().NotContainKey("ETag");
        headers.Should().ContainKey("Last-Modified").WhoseValue.Should().ContainSingle(v => v == "Mon, 13 Nov 2023 13:30:05 GMT");
    }

    [Fact]
    public void OnResultExecuting_WithObject_UpdatesHeaders()
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        IActionResult result = new OkObjectResult(new TableData() { Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68 }, UpdatedAt = DateTimeOffset.Parse("2023-11-13T13:30:05.1234Z") });

        ResultExecutingContext context = new(actionContext, filters, result, Substitute.For<Controller>());
        DatasyncControllerAttribute attribute = new();

        attribute.OnResultExecuting(context);

        context.HttpContext.Response.Headers.Should().ContainKey("ETag").WhoseValue.Should().ContainSingle(v => v == "\"YWJjZGVmZ2g=\"");
        context.HttpContext.Response.Headers.Should().ContainKey("Last-Modified").WhoseValue.Should().ContainSingle(v => v == "Mon, 13 Nov 2023 13:30:05 GMT");
    }

    [Fact]
    public void OnResultExecuting_NotITableData_Works()
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        IActionResult result = new OkObjectResult("This is a test");

        ResultExecutingContext context = new(actionContext, filters, result, Substitute.For<Controller>());
        DatasyncControllerAttribute attribute = new();

        attribute.OnResultExecuting(context);

        context.HttpContext.Response.Headers.Should().BeEmpty();
    }

    [Fact]
    public void OnResultExecuting_NoContent_Works()
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        IActionResult result = new NoContentResult();

        ResultExecutingContext context = new(actionContext, filters, result, Substitute.For<Controller>());
        DatasyncControllerAttribute attribute = new();

        attribute.OnResultExecuting(context);

        context.HttpContext.Response.Headers.Should().BeEmpty();
    }

    [Theory]
    [InlineData(304, false)]
    [InlineData(400, false)]
    [InlineData(401, false)]
    [InlineData(403, false)]
    [InlineData(404, false)]
    [InlineData(409, true)]
    [InlineData(412, true)]
    public void OnException_WithHttpException_Works(int statusCode, bool hasPayload)
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        TableData entity = new() { Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68 }, UpdatedAt = DateTimeOffset.Parse("2023-11-13T13:30:05.1234Z") };
        HttpException exception = new(statusCode);
        if (hasPayload) exception.Payload = entity;

        ExceptionContext context = new(actionContext, filters) { Exception = exception };
        DatasyncControllerAttribute attribute = new();

        attribute.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        if (hasPayload)
        {
            context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(statusCode);
            context.Result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeOfType<TableData>().And.BeEquivalentTo(entity);
            context.HttpContext.Response.Headers.Should().ContainKey("ETag").WhoseValue.Should().ContainSingle(v => v == "\"YWJjZGVmZ2g=\"");
            context.HttpContext.Response.Headers.Should().ContainKey("Last-Modified").WhoseValue.Should().ContainSingle(v => v == "Mon, 13 Nov 2023 13:30:05 GMT");
        }
        else
        {
            context.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(statusCode);
            context.HttpContext.Response.Headers.Should().BeEmpty();
        }
    }

    [Fact]
    public void OnException_DoesNotProcessOtherExceptions()
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        ExceptionContext context = new(actionContext, filters) { Exception = new ApplicationException() };
        DatasyncControllerAttribute attribute = new();

        attribute.OnException(context);

        context.ExceptionHandled.Should().BeFalse();
        context.HttpContext.Response.Headers.Should().BeEmpty();
    }

    [Fact]
    public void OnException_WithHttpException_WithoutITableData_DoesntSetHeaders()
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        const string entity = "foo";
        HttpException exception = new(400) { Payload = entity };

        ExceptionContext context = new(actionContext, filters) { Exception = exception };
        DatasyncControllerAttribute attribute = new();

        attribute.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(400);
        context.Result.Should().BeOfType<ObjectResult>().Which.Value.Should().BeOfType<string>().And.BeEquivalentTo(entity);
        context.HttpContext.Response.Headers.Should().BeEmpty();
    }

    [Fact]
    public void OnException_ExceptionHandled_DoesntDoAnything()
    {
        ActionContext actionContext = new() { HttpContext = new DefaultHttpContext(), RouteData = new Routing.RouteData(), ActionDescriptor = new ActionDescriptor() };
        List<IFilterMetadata> filters = new();
        actionContext.HttpContext.Response.Headers.Add("ETag", "\"foo\"");
        TableData entity = new() { Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68 }, UpdatedAt = DateTimeOffset.Parse("2023-11-13T13:30:05.1234Z") };
        HttpException exception = new(400) { Payload = entity };
        ExceptionContext context = new(actionContext, filters)
        {
            Exception = exception,
            ExceptionHandled = true,
            Result = new StatusCodeResult(200)
        };
        DatasyncControllerAttribute attribute = new();

        attribute.OnException(context);

        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(200);
        context.HttpContext.Response.Headers.Should().ContainKey("ETag").WhoseValue.Should().ContainSingle(v => v == "\"foo\"");
    }
}
