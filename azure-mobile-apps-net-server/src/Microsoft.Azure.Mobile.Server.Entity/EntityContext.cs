// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// The <see cref="EntityContext"/> is an abstract base class which provides the same functionality as the 
    /// <see cref="DbContext"/> scaffolded by Visual Studio. It is optional to use this base class 
    /// instead of the scaffolded code when using a <see cref="TableController{T}"/>.
    /// </summary>
    public abstract class EntityContext : DbContext
    {
        private const string DefaultNameOrConnectionStringName = "Name=MS_TableConnectionString";

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityContext"/> with the default schema name
        /// derived from the service name.
        /// </summary>
        protected EntityContext()
            : base(DefaultNameOrConnectionStringName)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }

            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }
    }
}