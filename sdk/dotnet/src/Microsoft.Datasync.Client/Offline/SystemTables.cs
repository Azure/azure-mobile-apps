// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.DeltaToken;
using Microsoft.Datasync.Client.Offline.Queue;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The names of the tables in the offline store that are reserved by the
    /// sync framework.
    /// </summary>
    public static class SystemTables
    {
        /// <summary>
        /// The prefix for all system tables.
        /// </summary>
        public const string Prefix = "__";

        /// <summary>
        /// The name of the configuration settings table.
        /// </summary>
        public const string Configuration = Prefix + "config";

        /// <summary>
        /// The name of the operations queue table.
        /// </summary>
        public const string OperationsQueue = Prefix + "operations";

        /// <summary>
        /// The name of the sync errors table.
        /// </summary>
        public const string SyncErrors = Prefix + "errors";

        /// <summary>
        /// The list of all the system tables.
        /// </summary>
        public static IEnumerable<string> AllTables { get; } = new[]
        {
            Configuration,
            OperationsQueue,
            SyncErrors
        };

        /// <summary>
        /// Defines all the system tables in an offline store.
        /// </summary>
        /// <param name="store">The offline store.</param>
        public static void DefineAllSystemTables(IOfflineStore store)
        {
            if (store is not IDeltaTokenStoreProvider)
            {
                store.DefineTable(Configuration, DefaultDeltaTokenStore.TableDefinition);
            }

            store.DefineTable(OperationsQueue, TableOperation.TableDefinition);
            store.DefineTable(SyncErrors, TableOperationError.TableDefinition);
        }
    }
}
