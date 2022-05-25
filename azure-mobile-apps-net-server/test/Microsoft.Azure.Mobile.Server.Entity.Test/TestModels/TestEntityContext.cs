// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Data.Entity;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class TestEntityContext : EntityContext
    {
        public TestEntityContext()
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
}
