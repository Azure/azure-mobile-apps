// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Data.Entity;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class TestEntityModelContext : EntityContext
    {
        public DbSet<TestEntityModel> TestEntityModels { get; set; }
    }
}
