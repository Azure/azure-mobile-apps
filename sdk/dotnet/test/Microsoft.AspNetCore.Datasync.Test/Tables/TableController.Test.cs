// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.Datasync.Test.Tables;

[ExcludeFromCodeCoverage]
public class TableController_Tests
{
    [Fact]
    public void Repository_Throws_WhenSetNull()
    {
        var controller = new TableController<InMemoryMovie>();
        Assert.Throws<ArgumentNullException>(() => controller.Repository = null);
    }

    [Fact]
    public void Repository_Throws_WhenGetNull()
    {
        var controller = new TableController<InMemoryMovie>();
        Assert.Throws<InvalidOperationException>(() => controller.Repository);
    }

    [Fact]
    public void Repository_CanBeStored()
    {
        var repository = new InMemoryRepository<InMemoryMovie>();
        var controller = new TableController<InMemoryMovie>() { Repository = repository };
        Assert.NotNull(controller.Repository);
        Assert.Equal(repository, controller.Repository);
    }

    [Fact]
    public void Ctor_Accepts_EdmModel()
    {
        var repository = new InMemoryRepository<InMemoryMovie>();
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.AddEntityType(typeof(InMemoryMovie));

        var controller = new TableController<InMemoryMovie>(repository, null, modelBuilder.GetEdmModel(), null);
        controller.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_Throws_InvalidEdmModel()
    {
        var repository = new InMemoryRepository<InMemoryMovie>();
        var modelBuilder = new ODataConventionModelBuilder();

        Action act = () => _ = new TableController<InMemoryMovie>(repository, null, modelBuilder.GetEdmModel(), null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsClientSideEvaluationException_Works()
    {
        Assert.False(TableController<InMemoryMovie>.IsClientSideEvaluationException(null));
        Assert.True(TableController<InMemoryMovie>.IsClientSideEvaluationException(new InvalidOperationException()));
        Assert.True(TableController<InMemoryMovie>.IsClientSideEvaluationException(new NotSupportedException()));
        Assert.False(TableController<InMemoryMovie>.IsClientSideEvaluationException(new ApplicationException()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CatchClientSideEvaluationException_RethrowsInnerException(bool hasLogger)
    {
        var repository = new InMemoryRepository<InMemoryMovie>();
        var controller = new TableController<InMemoryMovie>() { Repository = repository };
        if (hasLogger) controller.Logger = new NullLogger<TableController<InMemoryMovie>>();

        static void evaluator() { throw new ApplicationException(); }

        Assert.Throws<ApplicationException>(() => controller.CatchClientSideEvaluationException(new NotSupportedException(), "foo", evaluator));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CatchClientSideEvaluationException_ThrowsOriginalException(bool hasLogger)
    {
        var repository = new InMemoryRepository<InMemoryMovie>();
        var controller = new TableController<InMemoryMovie>() { Repository = repository };
        if (hasLogger) controller.Logger = new NullLogger<TableController<InMemoryMovie>>();
        var exception = new ApplicationException();

        static void evaluator() { throw new ApplicationException(); }

        var actual = Assert.Throws<ApplicationException>(() => controller.CatchClientSideEvaluationException(exception, "foo", evaluator));
        Assert.Same(exception, actual);
    }
}
