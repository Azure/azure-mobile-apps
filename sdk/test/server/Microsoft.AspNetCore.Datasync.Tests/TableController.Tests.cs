// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;

namespace Microsoft.AspNetCore.Datasync.Tests;

// CA2012 is fired for Substitute.For<> that returns a ValueTask, which is not a problem.
#pragma warning disable CA2012 // Use ValueTasks correctly

[ExcludeFromCodeCoverage]
public class TableController_Tests
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
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
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
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
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
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
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
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
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
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
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
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        ExposedTableController<TableData> controller = new(repository, provider);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task AuthorizeRequestAsync_ThrowsIfNotAuthorized()
    {
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
        provider.IsAuthorizedAsync(Arg.Any<TableOperation>(), Arg.Any<TableData>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult(false));
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        ExposedTableController<TableData> controller = new(repository, provider);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task AuthorizeRequestAsync_ThrowsIfNotAuthorized_ViaOptions()
    {
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
        provider.IsAuthorizedAsync(Arg.Any<TableOperation>(), Arg.Any<TableData>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult(false));
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        TableControllerOptions options = new() { UnauthorizedStatusCode = 403 };
        ExposedTableController<TableData> controller = new(repository, provider, options);

        Func<Task> act = async () => await controller.__AuthorizeRequestAsync(TableOperation.Create, new TableData(), CancellationToken.None);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(403);
    }

    [Fact]
    public async Task AuthorizeRequestAsync_AllowsIfAuthorized()
    {
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
        provider.IsAuthorizedAsync(Arg.Any<TableOperation>(), Arg.Any<TableData>(), Arg.Any<CancellationToken>()).Returns(ValueTask.FromResult(true));
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
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
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
        provider.IsAuthorizedAsync(Arg.Any<TableOperation>(), Arg.Any<TableData>(), Arg.Any<CancellationToken>()).ThrowsAsync(new ApplicationException());
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
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
        IAccessControlProvider<TableData> provider = Substitute.For<IAccessControlProvider<TableData>>();
        provider.IsAuthorizedAsync(Arg.Any<TableOperation>(), Arg.Any<TableData>(), Arg.Any<CancellationToken>()).ThrowsAsync(new ApplicationException());
        IRepository<TableData> repository = Substitute.For<IRepository<TableData>>();
        ExposedTableController<TableData> controller = new(repository, provider);
        TableData entity = new() { Id = "1" };
        List<RepositoryUpdatedEventArgs> firedEvents = new();
        controller.RepositoryUpdated += (_, e) => firedEvents.Add(e);

        Func<Task> act = async () => await controller.__PostCommitHookAsync(op, entity, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await provider.ReceivedWithAnyArgs(1).PostCommitHookAsync(default, default, default);
        firedEvents.Should().ContainSingle();
        firedEvents[0].Operation.Should().Be(op);
        firedEvents[0].EntityName.Should().Be("TableData");
        (firedEvents[0].Entity as TableData)?.Id.Should().Be("1");
        firedEvents[0].Timestamp.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }
    #endregion

    #region CreateAsync
    [Fact]
    public async Task CreateAsync_InvalidModel_Throws()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateAsync_Unauthorized_Throws()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateAsync_Conflict_Throws()
    {
        throw new NotImplementedException();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("6693aa44-f810-4b52-8e18-90c3facf807a")]
    public async Task CreateAsync_Works(string id)
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateAsync_StoresAfterPreCommitHook()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateAsync_StoresBeforePostCommitHook()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region DeleteAsync
    #endregion

    #region ReadAsync
    #endregion

    #region ReplaceAsync
    #endregion
}
