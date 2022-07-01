// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync.InMemory;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
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
        public void IsClientSideEvaluationException_Works()
        {
            Assert.False(TableController<InMemoryMovie>.IsClientSideEvaluationException(null));
            Assert.True(TableController<InMemoryMovie>.IsClientSideEvaluationException(new InvalidOperationException()));
            Assert.True(TableController<InMemoryMovie>.IsClientSideEvaluationException(new NotSupportedException()));
            Assert.False(TableController<InMemoryMovie>.IsClientSideEvaluationException(new ApplicationException()));
        }

        [Fact]
        public void CatchClientSideEvaluationException_RethrowsInnerException()
        {
            var repository = new InMemoryRepository<InMemoryMovie>();
            var controller = new TableController<InMemoryMovie>() { Repository = repository };

            static void evaluator() { throw new ApplicationException(); }

            Assert.Throws<ApplicationException>(() => controller.CatchClientSideEvaluationException(new NotSupportedException(), "foo", evaluator));
        }
    }
}
