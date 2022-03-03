// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Operations;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The names of the tables in the offline store that are reserved by the
    /// sync framework.
    /// </summary>
    public static class OfflineSystemTables
    {
        /// <summary>
        /// The name of the configuration settings table.
        /// </summary>
        public const string Configuration = "__config";

        /// <summary>
        /// The name of the operations queue table.
        /// </summary>
        public const string OperationsQueue = "__operations";

        /// <summary>
        /// The name of the synchronization errors table.
        /// </summary>
        public const string SyncErrors = "__errors";

        /// <summary>
        /// The list of all the system tables.
        /// </summary>
        public static IEnumerable<string> All { get; } = new[]
        {
            Configuration,
            OperationsQueue,
            SyncErrors
        };

        /// <summary>
        /// Define all the tables that are used by the sync service in the offline store.
        /// </summary>
        /// <param name="store">The offline store.</param>
        public static void DefineAllSystemTableTables(AbstractOfflineStore store)
        {
            store.DefineTable(Configuration, SyncSettingsManager.TableDefinition);
            store.DefineTable(OperationsQueue, TableOperation.TableDefinition);
            store.DefineTable(SyncErrors, TableOperationError.TableDefinition);
        }
    }
}
