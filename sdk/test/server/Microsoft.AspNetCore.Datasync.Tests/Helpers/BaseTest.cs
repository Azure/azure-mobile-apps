// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute.ExceptionExtensions;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Web;

// CA2012 is fired for Substitute.For<> that returns a ValueTask, which is not a problem.
#pragma warning disable CA2012 // Use ValueTasks correctly

namespace Microsoft.AspNetCore.Datasync.Tests.Helpers;

[ExcludeFromCodeCoverage]
public abstract class BaseTest
{
    protected static IAccessControlProvider<TEntity> FakeAccessControlProvider<TEntity>(TableOperation operation, bool isAuthorized, Expression<Func<TEntity, bool>> filter = null) where TEntity : class, ITableData
    {
        var mock = Substitute.For<IAccessControlProvider<TEntity>>();
        mock.IsAuthorizedAsync(operation, Arg.Any<TEntity>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult(isAuthorized));
        mock.GetDataView().Returns(filter);
        mock.PreCommitHookAsync(operation, Arg.Any<TEntity>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        mock.PostCommitHookAsync(operation, Arg.Any<TEntity>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        return mock;
    }

    protected static IRepository<TEntity> FakeRepository<TEntity>(TEntity entity = null, bool throwConflict = false) where TEntity : class, ITableData
    {
        var mock = Substitute.For<IRepository<TEntity>>();
        if (throwConflict)
        {
            mock.CreateAsync(Arg.Any<TEntity>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromException(new HttpException(409)));
            mock.DeleteAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromException(new HttpException(409)));
            mock.ReplaceAsync(Arg.Any<TEntity>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromException(new HttpException(409)));
        }
        else
        {
            mock.CreateAsync(Arg.Any<TEntity>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
            mock.DeleteAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
            mock.ReplaceAsync(Arg.Any<TEntity>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>()).Returns(ValueTask.CompletedTask);
        }

        if (entity == null)
        {
            mock.AsQueryableAsync(Arg.Any<CancellationToken>()).Throws(new HttpException(500));
            mock.ReadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws(new HttpException(404));
        }
        else
        {
            mock.AsQueryableAsync(Arg.Any<CancellationToken>()).Returns(new TEntity[] { entity }.AsQueryable());
            mock.ReadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult<TEntity>(entity));
        }
        return mock;
    }

    protected static HttpContext CreateHttpContext(HttpMethod method, string uri, Dictionary<string, string> headers = null)
    {
        Uri requestUri = new(uri);
        DefaultHttpContext context = new();
        context.Request.Method = method.ToString();
        context.Request.Scheme = requestUri.Scheme;
        context.Request.Path = requestUri.AbsolutePath;
        context.Request.Host = new HostString(requestUri.Host);
        context.Request.QueryString = new QueryString(requestUri.Query);

        NameValueCollection nvc = HttpUtility.ParseQueryString(requestUri.Query.TrimStart('?'));
        Dictionary<string, StringValues> dict = nvc.AllKeys.ToDictionary(k => k, k => new StringValues(nvc[k]));
        context.Request.Query = new QueryCollection(dict);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                context.Request.Headers.Add(header.Key, new StringValues(header.Value));
            }
        }

        return context;
    }
}
