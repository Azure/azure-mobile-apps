// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.ModelBuilder;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;

namespace Microsoft.AspNetCore.Datasync.Tests.Tables;

[ExcludeFromCodeCoverage]
public class TableController_Tests : BaseTest
{
    #region Test Artifacts
    // An implementation of TableController that exposes the protected methods
    class ExposedTableController<TEntity> : TableController<TEntity> where TEntity : class, ITableData
    {
        public ExposedTableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> provider) : base(repository, provider)
        {
            ObjectValidator = new ObjectValidator();
        }
        public ExposedTableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> provider, TableControllerOptions options) : base(repository, provider, options)
        {
            ObjectValidator = new ObjectValidator();
        }

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
        controller.EdmModel.Should().NotBeNull();
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
        controller.EdmModel.Should().NotBeNull();
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
        controller.EdmModel.Should().NotBeNull();
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
        controller.EdmModel.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_Repository_EdmModel_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        ODataConventionModelBuilder modelBuilder = new();
        modelBuilder.EnableLowerCamelCase();
        modelBuilder.AddEntityType(typeof(TableData));
        TableController<TableData> controller = new(repository, modelBuilder.GetEdmModel());

        controller.AccessControlProvider.Should().BeOfType<AccessControlProvider<TableData>>().And.NotBeNull();
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().NotBeNull();
        controller.Repository.Should().BeSameAs(repository);
        controller.EdmModel.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_Repository_AccessProvider_EdmModel_Works()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        IAccessControlProvider<TableData> provider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        ODataConventionModelBuilder modelBuilder = new();
        modelBuilder.EnableLowerCamelCase();
        modelBuilder.AddEntityType(typeof(TableData));
        TableController<TableData> controller = new(repository, provider, modelBuilder.GetEdmModel());

        controller.AccessControlProvider.Should().BeSameAs(provider);
        controller.Logger.Should().BeOfType<NullLogger>().And.NotBeNull();
        controller.Options.Should().NotBeNull();
        controller.Repository.Should().BeSameAs(repository);
        controller.EdmModel.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_Repository_EdmModel_Throws_WhenEntityNotFound()
    {
        IRepository<TableData> repository = FakeRepository<TableData>();
        ODataConventionModelBuilder modelBuilder = new();
        modelBuilder.EnableLowerCamelCase();
        modelBuilder.AddEntityType(typeof(InMemoryMovie));
        Action act = () => _ = new TableController<TableData>(repository, modelBuilder.GetEdmModel());
        act.Should().Throw<InvalidOperationException>();
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
        controller.EdmModel.Should().NotBeNull();
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
        controller.EdmModel.Should().NotBeNull();
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
        firedEvents[0].Operation.Should().Be(op);
        firedEvents[0].Entity.Should().BeEquivalentTo(entity);
        firedEvents[0].EntityName.Should().Be("TableData");
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
        TableData entity = new();
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Post, "https://localhost/table", entity);

        Func<Task> act = async () => await controller.CreateAsync();

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task CreateAsync_RepositoryException_Throws()
    {
        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        TableData entity = new();
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Post, "https://localhost/table", entity);

        Func<Task> act = async () => await controller.CreateAsync();

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409);
    }

    [Fact]
    public async Task CreateAsync_NonJsonData_Throws()
    {
        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateNonJsonHttpContext(HttpMethod.Post, "https://localhost/table");

        Func<Task> act = async () => await controller.CreateAsync();

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(415);
    }

    [Fact]
    public async Task CreateAsync_Works()
    {
        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Create, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Post, "https://localhost/table", entity);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        CreatedAtRouteResult actual = await controller.CreateAsync() as CreatedAtRouteResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(201);
        actual.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(entity.Id);

        await accessProvider.Received(1).PreCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().ContainSingle();

        await repository.Received(1).CreateAsync(Arg.Any<TableData>(), Arg.Any<CancellationToken>());
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

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true, m => m.Id == "1");
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.DeleteAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task DeleteAsync_Unauthorized_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, false);
        IRepository<TableData> repository = FakeRepository(entity, false);
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
        IRepository<TableData> repository = FakeRepository(entity, false);
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
            Deleted = false
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", "\"foo\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Sun, 12 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        TableControllerOptions options = new();
        IRepository<TableData> repository = FakeRepository(entity, false);
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
            Deleted = false
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", $"\"{Convert.ToBase64String(entity.Version)}\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Fri, 17 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Delete, true);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}", headers);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        NoContentResult actual = await controller.DeleteAsync(entity.Id) as NoContentResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(204);

        await accessProvider.Received(1).PreCommitHookAsync(TableOperation.Update, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Update, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
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
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Delete, $"https://localhost/table/{entity.Id}", headers);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        NoContentResult actual = await controller.DeleteAsync(entity.Id) as NoContentResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(204);

        await accessProvider.Received(0).PreCommitHookAsync(TableOperation.Delete, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Delete, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().ContainSingle();

        await repository.Received(1).ReadAsync(entity.Id, Arg.Any<CancellationToken>());
        await repository.Received(1).DeleteAsync(entity.Id, Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }
    #endregion

    #region ReadAsync
    [Fact]
    public async Task ReadAsync_RepositoryException_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Read, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.ReadAsync("1");

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task ReadAsync_EntityNotInView_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Read, true, m => m.Id == "1");
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.ReadAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task ReadAsync_Unauthorized_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Read, false);
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.ReadAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task ReadAsync_SoftDeleted_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4", Deleted = true };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Read, true);
        IRepository<TableData> repository = FakeRepository(entity, false);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.ReadAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(410);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ReadAsync_PreconditionFailed_Throws(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = false
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-None-Match", $"\"{Convert.ToBase64String(entity.Version)}\"");
        if (includeLastModified) headers.Add("If-Modified-Since", "Wed, 15 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Read, true);
        TableControllerOptions options = new();
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, $"https://localhost/table/{entity.Id}", headers);

        Func<Task> act = async () => await controller.ReadAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(304);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ReadAsync_Works(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = false
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-None-Match", "\"foo\"");
        if (includeLastModified) headers.Add("If-Modified-Since", "Sun, 12 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Read, true);
        TableControllerOptions options = new();
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, $"https://localhost/table/{entity.Id}", headers);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        OkObjectResult actual = await controller.ReadAsync(entity.Id) as OkObjectResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(200);
        actual.Value.Should().BeEquivalentTo(entity);

        await accessProvider.Received(0).PreCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(0).PostCommitHookAsync(TableOperation.Create, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().BeEmpty();

        await repository.Received(1).ReadAsync(entity.Id, Arg.Any<CancellationToken>());
    }
    #endregion

    #region ReplaceAsync
    [Fact]
    public async Task ReplaceAsync_IdMismatch_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity);

        Func<Task> act = async () => await controller.ReplaceAsync("1");

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(400);
    }

    [Fact]
    public async Task ReplaceAsync_NonJson_Throws()
    {
        TableData entity = new() { Id = "cac9b793-000c-450e-8477-663522b14727" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateNonJsonHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}");

        Func<Task> act = async () => await controller.ReplaceAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(415);
    }

    [Fact]
    public async Task ReplaceAsync_RepositoryException_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity);

        Func<Task> act = async () => await controller.ReplaceAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task ReplaceAsync_EntityNotInView_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true, m => m.Id == "1");
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity);

        Func<Task> act = async () => await controller.ReplaceAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [Fact]
    public async Task ReplaceAsync_Unauthorized_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, false);
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity);

        Func<Task> act = async () => await controller.ReplaceAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task ReplaceAsync_SoftDeleted_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4", Deleted = true };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true);
        IRepository<TableData> repository = FakeRepository(entity, false);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity);

        Func<Task> act = async () => await controller.ReplaceAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(410);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ReplaceAsync_PreconditionFailed_Throws(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = false
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", "\"foo\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Sun, 12 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true);
        TableControllerOptions options = new();
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity, headers);

        Func<Task> act = async () => await controller.ReplaceAsync(entity.Id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(entity);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ReplaceAsync_Works(bool includeIfMatch, bool includeLastModified)
    {
        TableData entity = new()
        {
            Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4",
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 },
            UpdatedAt = new DateTimeOffset(2023, 11, 13, 12, 30, 05, TimeSpan.Zero),
            Deleted = false
        };

        Dictionary<string, string> headers = new();
        if (includeIfMatch) headers.Add("If-Match", $"\"{Convert.ToBase64String(entity.Version)}\"");
        if (includeLastModified) headers.Add("If-Unmodified-Since", "Wed, 15 Nov 2023 07:28:00 GMT");

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Update, true);
        TableControllerOptions options = new();
        IRepository<TableData> repository = FakeRepository(entity, false);
        ExposedTableController<TableData> controller = new(repository, accessProvider, options);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Put, $"https://localhost/table/{entity.Id}", entity, headers);
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        OkObjectResult actual = await controller.ReplaceAsync(entity.Id) as OkObjectResult;

        actual.Should().NotBeNull();
        actual.StatusCode.Should().Be(200);
        actual.Value.Should().BeEquivalentTo(entity);

        await accessProvider.Received(1).PreCommitHookAsync(TableOperation.Update, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        await accessProvider.Received(1).PostCommitHookAsync(TableOperation.Update, Arg.Any<TableData>(), Arg.Any<CancellationToken>());
        firedEvents.Should().ContainSingle();

        await repository.Received(1).ReadAsync(entity.Id, Arg.Any<CancellationToken>());
        await repository.Received(1).ReplaceAsync(Arg.Any<TableData>(), Arg.Any<byte[]>(), Arg.Any<CancellationToken>());
    }
    #endregion
}
