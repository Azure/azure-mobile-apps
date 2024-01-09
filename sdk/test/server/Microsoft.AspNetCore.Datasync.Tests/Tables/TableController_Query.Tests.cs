using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.Datasync.Tests.Tables;

#pragma warning disable RCS1163 // Unused parameter

[ExcludeFromCodeCoverage]
public class TableController_Query_Tests : BaseTest
{
    #region BuildPagedResults
    [Fact]
    public void BuildPagedResult_NulLArg_BuildsPagedResult()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };

        // Build an ODataQueryOptions with no query
        var builder = new ODataConventionModelBuilder();
        builder.AddEntityType(typeof(InMemoryMovie));
        IEdmModel model = builder.GetEdmModel();
        var queryContext = new ODataQueryContext(model, typeof(InMemoryMovie), new Microsoft.OData.UriParser.ODataPath());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        var queryOptions = new ODataQueryOptions(queryContext, httpContext.Request);

        var result = controller.BuildPagedResult(queryOptions, null, 0);
        result.Should().NotBeNull();
        result.Items.Count().Should().Be(0);
        result.Count.Should().BeNull();
    }
    #endregion

    #region CatchClientSideEvaluationException
    [Fact]
    public void CatchClientSideEvaluationException_NotCCEE_ThrowsOriginalException()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        var exception = new ApplicationException("Original exception");
        static void evaluator() { throw new ApplicationException("In evaluator"); }
        Action act = () => controller.CatchClientSideEvaluationException(exception, "foo", evaluator);
        act.Should().Throw<ApplicationException>().WithMessage("Original exception");
    }

    [Fact]
    public void CatchClientSideEvaluationException_NotCCEE_WithInner_ThrowsOriginalException()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        var exception = new ApplicationException("Original exception", new ApplicationException());
        static void evaluator() { throw new ApplicationException("In evaluator"); }
        Action act = () => controller.CatchClientSideEvaluationException(exception, "foo", evaluator);
        act.Should().Throw<ApplicationException>().WithMessage("Original exception");
    }

    [Fact]
    public void CatchClientSideEvaluationException_CCEE_ThrowsEvaluatorException()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        var exception = new NotSupportedException("Original exception", new ApplicationException("foo"));
        static void evaluator() { throw new ApplicationException("In evaluator"); }
        Action act = () => controller.CatchClientSideEvaluationException(exception, "foo", evaluator);
        act.Should().Throw<ApplicationException>().WithMessage("In evaluator");
    }

    [Fact]
    public void CatchClientSideEvaluationException_CCEEInner_ThrowsEvaluatorException()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        var exception = new ApplicationException("Original exception", new NotSupportedException("foo"));
        static void evaluator() { throw new ApplicationException("In evaluator"); }
        Action act = () => controller.CatchClientSideEvaluationException(exception, "foo", evaluator);
        act.Should().Throw<ApplicationException>().WithMessage("In evaluator");
    }

    [Fact]
    public void CatchClientSideEvaluationException_CCEE_ExecutesEvaluator()
    {
        bool isExecuted = false;
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        var exception = new NotSupportedException("Original exception", new ApplicationException("foo"));
        Action act = () => controller.CatchClientSideEvaluationException(exception, "foo", () => isExecuted = true);
        act.Should().NotThrow();
        isExecuted.Should().BeTrue();
    }

    [Fact]
    public void CatchClientSideEvaluationException_CCEEInner_ExecutesEvaluator()
    {
        bool isExecuted = false;
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        var exception = new ApplicationException("Original exception", new NotSupportedException("foo"));
        Action act = () => controller.CatchClientSideEvaluationException(exception, "foo", () => isExecuted = true);
        act.Should().NotThrow();
        isExecuted.Should().BeTrue();
    }
    #endregion

    #region ExecuteQueryWithClientEvaluation
    [Fact]
    public void ExecuteQueryWithClientEvaluation_ExecutesServiceSide()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        controller.Options.DisableClientSideEvaluation = true;

        int evaluations = 0;
        void evaluator(IQueryable<InMemoryMovie> dataset)
        {
            evaluations++;
            // if (evaluations == 1) throw new NotSupportedException("Server side");
            // if (evaluations == 2) throw new NotSupportedException("Client side");
        }
        List<InMemoryMovie> dataset = new();

        Action act = () => controller.ExecuteQueryWithClientEvaluation(dataset.AsQueryable(), evaluator);

        act.Should().NotThrow();
        evaluations.Should().Be(1);
    }

    [Fact]
    public void ExecuteQueryWithClientEvaluation_ThrowsServiceSide_WhenClientEvaluationDisabled()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        controller.Options.DisableClientSideEvaluation = true;

        int evaluations = 0;
        void evaluator(IQueryable<InMemoryMovie> dataset)
        {
            evaluations++;
            if (evaluations == 1) throw new NotSupportedException("Server side");
            if (evaluations == 2) throw new NotSupportedException("Client side");
        }
        List<InMemoryMovie> dataset = new();

        Action act = () => controller.ExecuteQueryWithClientEvaluation(dataset.AsQueryable(), evaluator);

        act.Should().Throw<NotSupportedException>().WithMessage("Server side");
    }

    [Fact]
    public void ExecuteQueryWithClientEvaluation_ExecutesClientSide_WhenClientEvaluationEnabled()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        controller.Options.DisableClientSideEvaluation = false;

        int evaluations = 0;
        void evaluator(IQueryable<InMemoryMovie> dataset)
        {
            evaluations++;
            if (evaluations == 1) throw new NotSupportedException("Server side");
            //if (evaluations == 2) throw new NotSupportedException("Client side");
        }
        List<InMemoryMovie> dataset = new();

        Action act = () => controller.ExecuteQueryWithClientEvaluation(dataset.AsQueryable(), evaluator);

        act.Should().NotThrow();
        evaluations.Should().Be(2);
    }

    [Fact]
    public void ExecuteQueryWithClientEvaluation_ThrowsClientSide_WhenClientEvaluationEnabled()
    {
        var controller = new TableController<InMemoryMovie>() { Repository = new InMemoryRepository<InMemoryMovie>() };
        controller.Options.DisableClientSideEvaluation = false;

        int evaluations = 0;
        void evaluator(IQueryable<InMemoryMovie> dataset)
        {
            evaluations++;
            if (evaluations == 1) throw new NotSupportedException("Server side", new ApplicationException("Inner exception"));
            if (evaluations == 2) throw new NotSupportedException("Client side");
        }
        List<InMemoryMovie> dataset = new();

        Action act = () => controller.ExecuteQueryWithClientEvaluation(dataset.AsQueryable(), evaluator);

        act.Should().Throw<NotSupportedException>().WithMessage("Client side");
        evaluations.Should().Be(2);
    }
    #endregion

    [Fact]
    public void IsClientSideEvaluationException_Works()
    {
        Assert.False(TableController<InMemoryMovie>.IsClientSideEvaluationException(null));
        Assert.True(TableController<InMemoryMovie>.IsClientSideEvaluationException(new InvalidOperationException()));
        Assert.True(TableController<InMemoryMovie>.IsClientSideEvaluationException(new NotSupportedException()));
        Assert.False(TableController<InMemoryMovie>.IsClientSideEvaluationException(new ApplicationException()));
    }

    #region QueryAsync
    [Fact]
    public async Task QueryAsync_Unauthorized_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Query, false);
        IRepository<TableData> repository = FakeRepository(entity, true);
        TableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, "https://localhost/table");

        Func<Task> act = async () => await controller.QueryAsync();

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(401);
    }

    [Fact]
    public async Task QueryAsync_RepositoryException_Throws()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Query, true);
        IRepository<TableData> repository = FakeRepository<TableData>(null, true);
        TableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, "https://localhost/table");

        Func<Task> act = async () => await controller.QueryAsync();

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(500);
    }

    [Fact]
    public async Task QueryAsync_NoExtras_Works()
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Query, true);
        IRepository<TableData> repository = FakeRepository(entity, true);
        TableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, "https://localhost/table");

        OkObjectResult result = await controller.QueryAsync() as OkObjectResult;
        result.Should().NotBeNull();
        PagedResult pagedResult = result.Value as PagedResult;
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("0da7fb24-3606-442f-9f68-c47c6e7d09d4", 1)]
    [InlineData("1", 0)]
    public async Task QueryAsync_DataView_Works(string filter, int count)
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4" };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Query, true, m => m.Id == filter);
        IRepository<TableData> repository = FakeRepository(entity, true);
        TableController<TableData> controller = new(repository, accessProvider);
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, "https://localhost/table");

        OkObjectResult result = await controller.QueryAsync() as OkObjectResult;
        result.Should().NotBeNull();
        PagedResult pagedResult = result.Value as PagedResult;
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(count);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task QueryAsync_DeletedSkipped_Works(bool isDeleted, int count)
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4", Deleted = isDeleted };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Query, true);
        IRepository<TableData> repository = FakeRepository(entity, true);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        TableController<TableData> controller = new(repository, accessProvider) { Options = options };
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, "https://localhost/table");

        OkObjectResult result = await controller.QueryAsync() as OkObjectResult;
        result.Should().NotBeNull();
        PagedResult pagedResult = result.Value as PagedResult;
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(count);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 1)]
    public async Task QueryAsync_DeletedIncluded_Works(bool isDeleted, int count)
    {
        TableData entity = new() { Id = "0da7fb24-3606-442f-9f68-c47c6e7d09d4", Deleted = isDeleted };

        IAccessControlProvider<TableData> accessProvider = FakeAccessControlProvider<TableData>(TableOperation.Query, true);
        IRepository<TableData> repository = FakeRepository(entity, true);
        TableControllerOptions options = new() { EnableSoftDelete = true };
        TableController<TableData> controller = new(repository, accessProvider) { Options = options };
        controller.ControllerContext.HttpContext = CreateHttpContext(HttpMethod.Get, "https://localhost/table?__includedeleted=true");

        OkObjectResult result = await controller.QueryAsync() as OkObjectResult;
        result.Should().NotBeNull();
        PagedResult pagedResult = result.Value as PagedResult;
        pagedResult.Should().NotBeNull();
        pagedResult.Items.Should().HaveCount(count);
    }
    #endregion
}