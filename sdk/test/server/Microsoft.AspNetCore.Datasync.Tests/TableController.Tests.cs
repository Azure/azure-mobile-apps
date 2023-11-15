// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Datasync.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Datasync.Tests;

// CA2012 is fired for Substitute.For<> that returns a ValueTask, which is not a problem.
#pragma warning disable CA2012 // Use ValueTasks correctly

[ExcludeFromCodeCoverage]
public class TableController_Tests : BaseTest
{
    #region Test Artifacts
    // An implementation of TableController that exposes the protected methods
    class ExposedTableController<TEntity> : TableController<TEntity> where TEntity : class, ITableData
    {
        public ExposedTableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> provider) : base(repository, provider) { }
        public ExposedTableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> provider, TableControllerOptions options) : base(repository, provider, options) { }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Used to indicate protected to public conversion")]
        public ValueTask __AuthorizeRequestAsync(TableOperation operation, TEntity entity, CancellationToken cancellationToken) => AuthorizeRequestAsync(operation, entity, cancellationToken);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Used to indicate protected to public conversion")]
        public ValueTask __PostCommitHookAsync(TableOperation operation, TEntity entity, CancellationToken cancellationTken) => PostCommitHookAsync(operation, entity, cancellationTken);
    }

    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    #endregion

    #region Constructors
    [Fact]
    public void Ctor_Empty_Works()
    {
        TableController<TableData> controller = new();

        controller.Should().NotBeNull();
        controller.AccessControlProvider.Should().BeOfType<AccessControlProvider<TableData>>().And.NotBeNull();
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().NotBeNull();
        controller.Repository.Should().BeOfType<Repository<TableData>>().And.NotBeNull();
    }

    [Fact]
    public void Ctor_Repository_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        TableController<TableData> controller = new(repository);

        controller.Should().NotBeNull();
        controller.AccessControlProvider.Should().BeOfType<AccessControlProvider<TableData>>().And.NotBeNull();
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().NotBeNull();
        controller.Repository.Should().BeSameAs(repository);
    }

    [Fact]
    public void Ctor_Repository_AccessProvider_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        TableController<TableData> controller = new(repository, provider);

        controller.Should().NotBeNull();
        controller.AccessControlProvider.Should().BeSameAs(provider);
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().NotBeNull();
        controller.Repository.Should().BeSameAs(repository);
    }

    [Fact]
    public void Ctor_Repository_Options_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        TableControllerOptions options = new();
        TableController<TableData> controller = new(repository, options);

        controller.Should().NotBeNull();
        controller.AccessControlProvider.Should().BeOfType<AccessControlProvider<TableData>>().And.NotBeNull();
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().BeSameAs(options);
        controller.Repository.Should().BeSameAs(repository);
    }

    [Fact]
    public void Ctor_Repository_AccessProvider_Options_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        TableControllerOptions options = new();
        TableController<TableData> controller = new(repository, provider, options);

        controller.Should().NotBeNull();
        controller.AccessControlProvider.Should().BeSameAs(provider);
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().BeSameAs(options);
        controller.Repository.Should().BeSameAs(repository);
    }

    [Fact]
    public void Ctor_Repository_AccessProvider_Options_Logger_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        TableControllerOptions options = new();
        ILogger logger = Substitute.For<ILogger>();
        TableController<TableData> controller = new(repository, provider, options) { Logger = logger };

        controller.Should().NotBeNull();
        controller.AccessControlProvider.Should().BeSameAs(provider);
        controller.Logger.Should().BeSameAs(logger);
        controller.Options.Should().BeSameAs(options);
        controller.Repository.Should().BeSameAs(repository);
    }
    #endregion

    #region AuthorizeRequestAsync
    [Fact]
    public async Task AuthorizeRequestAsync_ThrowsFromAccessControlProvider()
    {
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
        provider.IsAuthorizedAsync(Arg.Any<TableOperation>(), Arg.Any<TableData>(), Arg.Any<CancellationToken>()).ThrowsAsync(new ApplicationException());
        IRepository<TableData> repository = FakeRepository<TableData>();
        ExposedTableController<TableData> controller = new(repository, provider);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task AuthorizeRequestAsync_ThrowsIfNotAuthorized()
    {
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, false);
        IRepository<TableData> repository = FakeRepository<TableData>();
        ExposedTableController<TableData> controller = new(repository, provider);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task AuthorizeRequestAsync_ThrowsIfNotAuthorized_ViaOptions()
    {
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, false);
        IRepository<TableData> repository = FakeRepository<TableData>();
        TableControllerOptions options = new() { UnauthorizedStatusCode = 403 };
        ExposedTableController<TableData> controller = new(repository, provider, options);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(403);
    }

    [Fact]
    public async Task AuthorizeRequestAsync_AllowsIfAuthorized()
    {
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        IRepository<TableData> repository = FakeRepository<TableData>();
        ExposedTableController<TableData> controller = new(repository, provider);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
    #endregion

    #region PostCommitHookAsync
    [Theory]
    [InlineData(TableOperation.Create)]
    [InlineData(TableOperation.Delete)]
    [InlineData(TableOperation.Update)]
    public async Task PostCommitHookAsync_NoRepositoryUpdated(TableOperation op)
    {
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(op, true);
        IRepository<TableData> repository = FakeRepository<TableData>();
        ExposedTableController<TableData> controller = new(repository, provider);
        TableData entity = new() { Id = "1" };

        Func<Task> act = async () => await controller.__PostCommitHookAsync(op, entity, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await provider.ReceivedWithAnyArgs(1).PostCommitHookAsync(default, default, default);
    }

    [Theory]
    [InlineData(TableOperation.Create)]
    [InlineData(TableOperation.Delete)]
    [InlineData(TableOperation.Update)]
    public async Task PostCommitHookAsync_FiresRepositoryUpdated(TableOperation op)
    {
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(op, true);
        IRepository<TableData> repository = FakeRepository<TableData>();
        ExposedTableController<TableData> controller = new(repository, provider);
        TableData entity = new() { Id = "1" };
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        Func<Task> act = async () => await controller.__PostCommitHookAsync(op, entity, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await provider.ReceivedWithAnyArgs(1).PostCommitHookAsync(default, default, default);
        firedEvents.Should().ContainSingle();
        firedEvents[0].Should().BeEquivalentTo(new RepositoryUpdatedEventArgs(op, "TableData", entity), opt => opt.Excluding(m => m.Timestamp));
        firedEvents[0].Timestamp.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }
    #endregion

    #region CreateAsync
    [Fact]
    public async Task CreateAsync_Unauthorized_Throws()
    {
        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Create, false);
        IRepository<TableData> repository = FakeRepository<TableData>();
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Post, "https://localhost/table");

        Func<Task> act = async () => await controller.CreateAsync(new TableData());

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task CreateAsync_RepositoryException_Throws()
    {
        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Create, false);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Post, "https://localhost/table");

        Func<Task> act = async () => await controller.CreateAsync(new TableData());

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409);
    }

    [Fact]
    public async Task CreateAsync_Works()
    {
        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Post, "https://localhost/table");
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        CreatedAtActionResult actual = await controller.CreateAsync(entity) as CreatedAtActionResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(201);
        actual.ActionName.Should().Be("ReadAsync");
        actual.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(entity.Id);

        await accessProvider.Received(1).PreCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().ContainSingle();
    }
    #endregion

    #region DeleteAsync
    [Fact]
    public async Task DeleteAsync_RepositoryException_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.DeleteAsync("1");

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task DeleteAsync_EntityNotInView_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        IRepository<TableData> repository = FakeRepository<TableData>(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}");
        Expression<Func<TableData, bool>> expression = m => m.Id == "1";
        accessProvider.GetDataView().Returns(expression);

        Func<Task> act = async () => await controller.DeleteAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task DeleteAsync_Unauthorized_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, false);
        IRepository<TableData> repository = FakeRepository<TableData>(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.DeleteAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeleted_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4", Deleted = true };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        IRepository<TableData> repository = FakeRepository<TableData>(entity, false);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.DeleteAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(410);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task DeleteAsync_PreconditionFailed_Throws(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = true
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", "\"foo\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Wed, 15 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        IRepository<TableData> repository = FakeRepository<TableData>(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}", headers);

        Func<Task> act = async () => await controller.DeleteAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(entity);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task DeleteAsync_SoftDelete_Works(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = true
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", $"\"{Convert.ToBase64String(entity.Version)}\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Sun, 12 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        IRepository<TableData> repository = FakeRepository<TableData>(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}", headers);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        NoContentResult actual = await controller.DeleteAsync(entity.Id) as NoContentResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(204);

        await accessProvider.Received(1).PreCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().ContainSingle();

        await repository.Received(1).ReadAsync(entity.Id, Arg.Any<CancellationToken>());
        await repository.Received(1).ReplaceAsync(entity, Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DeleteAsync_HardDelete_Works(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = true
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", $"\"{Convert.ToBase64String(entity.Version)}\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Sun, 12 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        TableControllerOptions options = new() { EnableSoftDelete = false };
        IRepository<TableData> repository = FakeRepository<TableData>(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}", headers);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        NoContentResult actual = await controller.DeleteAsync(entity.Id) as NoContentResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(204);

        await accessProvider.Received(1).PreCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().ContainSingle();

        await repository.Received(1).ReadAsync(entity.Id, Arg.Any<CancellationToken>());
        await repository.Received(1).DeleteAsync(entity.Id, Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }
    #endregion

    #region ReadAsync
    #endregion

    #region ReplaceAsync
    #endregion
}
