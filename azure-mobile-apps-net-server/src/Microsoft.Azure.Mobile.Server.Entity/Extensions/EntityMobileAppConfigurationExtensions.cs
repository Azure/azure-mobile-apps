// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.SqlServer;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Azure.Mobile.Server.Tables.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// </summary>
    public static class EntityMobileAppConfigurationExtensions
    {
        private const string SqlClientProvider = "System.Data.SqlClient";

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need this temporarily to enable our SQL generator.")]
        static EntityMobileAppConfigurationExtensions()
        {
            // Register our global SqlServerMigrationSqlGenerator if one is not already registered.
            // We register it as early as possible so that we avoid doing it at a point where EF
            // no longer accepts configuration changes.
            DbConfiguration.Loaded += (_, a) => a.ReplaceService<Func<MigrationSqlGenerator>>(
                (s, k) => SqlClientProvider.Equals(k as string, StringComparison.OrdinalIgnoreCase) && s().GetType() == typeof(SqlServerMigrationSqlGenerator)
                    ? () => new EntityTableSqlGenerator()
                    : s);
        }

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static MobileAppTableConfiguration AddEntityFramework(this MobileAppTableConfiguration config)
        {
            return config;
        }

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static MobileAppConfiguration AddTablesWithEntityFramework(this MobileAppConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.AddTables(
                new MobileAppTableConfiguration()
                    .MapTableControllers()
                    .AddEntityFramework());

            return config;
        }
    }
}