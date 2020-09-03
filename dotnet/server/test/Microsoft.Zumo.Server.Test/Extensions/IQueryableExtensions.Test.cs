// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Zumo.Server.Test.Extensions
{
    [TestClass]
    public class IQueryableExtensions_Tests
    {
        #region Test Data
        class Entity : ITableData
        {
            public Entity()
            {
                Id = Guid.NewGuid().ToString("N");
                Version = Guid.NewGuid().ToByteArray();
                UpdatedAt = DateTimeOffset.UtcNow;
            }
            public string Id { get; set; }
            public byte[] Version { get; set; }
            public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

            public DateTimeOffset UpdatedAt { get; set; }
            public bool Deleted { get; set; }
        }

        readonly List<Entity> entityData = new List<Entity>(new Entity[]
        {
            new Entity { Deleted = false },
            new Entity { Deleted = true  },
            new Entity { Deleted = false },
            new Entity { Deleted = false },
            new Entity { Deleted = true  },
            new Entity { Deleted = false },
            new Entity { Deleted = false },
            new Entity { Deleted = true  },
            new Entity { Deleted = false },
            new Entity { Deleted = false }
        });

        private readonly Dictionary<string, StringValues> IncludeDeletedQuery = new Dictionary<string, StringValues>()
        {
            { "__includedeleted", "true" }
        };
        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyDeletedFilter_Throws_NullTableOptions()
        {
            var httpContext = new DefaultHttpContext();
            _ = entityData.AsQueryable().ApplyDeletedFilter(null, httpContext.Request);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyDeletedFilter_Throws_NullRequest()
        {
            var options = new TableControllerOptions();
            _ = entityData.AsQueryable().ApplyDeletedFilter(options, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void ApplyDeletedFilter_HardDelete_NoQuery_DoesNotFilter()
        {
            var options = new TableControllerOptions { SoftDeleteEnabled = false };
            var httpContext = new DefaultHttpContext();
            var actual = entityData.AsQueryable().ApplyDeletedFilter(options, httpContext.Request).ToList();
            CollectionAssert.AreEqual(entityData, actual);
        }

        [TestMethod]
        public void ApplyDeletedFilter_HardDelete_Query_DoesNotFilter()
        {
            var options = new TableControllerOptions { SoftDeleteEnabled = false };
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(IncludeDeletedQuery);
            var actual = entityData.AsQueryable().ApplyDeletedFilter(options, httpContext.Request).ToList();
            CollectionAssert.AreEqual(entityData, actual);
        }

        [TestMethod]
        public void ApplyDeletedFilter_SoftDelete_NoQuery_Filters()
        {
            var options = new TableControllerOptions { SoftDeleteEnabled = true };
            var httpContext = new DefaultHttpContext();
            var actual = entityData.AsQueryable().ApplyDeletedFilter(options, httpContext.Request).ToList();
            Assert.IsFalse(actual.Any(m => m.Deleted));
            Assert.AreEqual(7, actual.Count());
        }

        [TestMethod]
        public void ApplyDeletedFilter_SoftDelete_Query_DoesNotFilter()
        {
            var options = new TableControllerOptions { SoftDeleteEnabled = true };
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(IncludeDeletedQuery);
            var actual = entityData.AsQueryable().ApplyDeletedFilter(options, httpContext.Request).ToList();
            CollectionAssert.AreEqual(entityData, actual);
        }
    }
}
