// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal sealed class LocalStoreChangeTracker : IMobileServiceLocalStore
    {
        private readonly IMobileServiceLocalStore store;
        private readonly StoreTrackingContext trackingContext;
        private readonly MobileServiceObjectReader objectReader;
        private StoreOperationsBatch operationsBatch;
        private readonly IMobileServiceEventManager eventManager;
        private int isBatchCompleted = 0;
        private readonly MobileServiceSyncSettingsManager settings;
        private bool trackRecordOperations;
        private bool trackBatches;

        public LocalStoreChangeTracker(IMobileServiceLocalStore store, StoreTrackingContext trackingContext, IMobileServiceEventManager eventManager, MobileServiceSyncSettingsManager settings)
        {
            Arguments.IsNotNull(store, nameof(store));
            Arguments.IsNotNull(trackingContext, nameof(trackingContext));
            Arguments.IsNotNull(eventManager, nameof(eventManager));
            Arguments.IsNotNull(settings, nameof(settings));

            this.objectReader = new MobileServiceObjectReader();
            this.store = store;
            this.trackingContext = trackingContext;
            this.eventManager = eventManager;
            this.settings = settings;

            InitializeTracking();
        }

        private void InitializeTracking()
        {
            this.trackRecordOperations = IsRecordTrackingEnabled();
            this.trackBatches = IsBatchTrackingEnabled();

            if (!this.trackRecordOperations & !this.trackBatches)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Tracking notifications are not enabled for the source {0}. To use a change tracker, you must enable record operation notifications, batch notifications or both.",
                    this.trackingContext.Source));
            }

            if (this.trackBatches)
            {
                this.operationsBatch = new StoreOperationsBatch(this.trackingContext.BatchId, this.trackingContext.Source);
            }
        }

        private bool IsBatchTrackingEnabled()
        {
            switch (trackingContext.Source)
            {
                case StoreOperationSource.Local:
                case StoreOperationSource.LocalPurge:
                case StoreOperationSource.LocalConflictResolution:
                    return false;
                case StoreOperationSource.ServerPull:
                    return trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPullBatch);
                case StoreOperationSource.ServerPush:
                    return trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPushBatch);
                default:
                    throw new InvalidOperationException("Unknown tracking source");
            }
        }

        private bool IsRecordTrackingEnabled()
        {
            switch (trackingContext.Source)
            {
                case StoreOperationSource.Local:
                case StoreOperationSource.LocalPurge:
                    return trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalOperations);
                case StoreOperationSource.LocalConflictResolution:
                    return trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalConflictResolutionOperations);
                case StoreOperationSource.ServerPull:
                    return trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPullOperations);
                case StoreOperationSource.ServerPush:
                    return trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPushOperations);
                default:
                    throw new InvalidOperationException("Unknown tracking source");
            }
        }

        public async Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            Arguments.IsNotNull(query, nameof(query));

            string[] recordIds = null;

            if (!query.TableName.StartsWith(MobileServiceLocalSystemTables.Prefix) && this.trackingContext.Source != StoreOperationSource.LocalPurge)
            {
                QueryResult result = await this.store.QueryAsync(query);
                recordIds = result.Values.Select(j => this.objectReader.GetId((JObject)j)).ToArray();
            }

            await this.store.DeleteAsync(query);

            if (recordIds != null)
            {
                foreach (var id in recordIds)
                {
                    TrackStoreOperation(query.TableName, id, LocalStoreOperationKind.Delete);
                }
            }
        }

        public async Task DeleteAsync(string tableName, IEnumerable<string> ids)
        {
            Arguments.IsNotNull(tableName, nameof(tableName));
            Arguments.IsNotNull(ids, nameof(ids));

            if (!tableName.StartsWith(MobileServiceLocalSystemTables.Prefix))
            {
                IEnumerable<string> notificationIds = ids;

                if (this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.DetectRecordChanges))
                {
                    IDictionary<string, string> existingRecords = await GetItemsAsync(tableName, ids, false);
                    notificationIds = existingRecords.Select(kvp => kvp.Key);
                }

                await this.store.DeleteAsync(tableName, ids);

                foreach (var id in notificationIds)
                {
                    TrackStoreOperation(tableName, id, LocalStoreOperationKind.Delete);
                }
            }
            else
            {
                await this.store.DeleteAsync(tableName, ids);
            }
        }

        public Task<JObject> LookupAsync(string tableName, string id)
        {
            Arguments.IsNotNull(tableName, nameof(tableName));
            Arguments.IsNotNull(id, nameof(id));
            return this.store.LookupAsync(tableName, id);
        }

        public Task<JToken> ReadAsync(MobileServiceTableQueryDescription query)
        {
            Arguments.IsNotNull(query, nameof(query));

            return this.store.ReadAsync(query);
        }

        public async Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns)
        {
            Arguments.IsNotNull(tableName, nameof(tableName));
            Arguments.IsNotNull(items, nameof(items));

            if (!tableName.StartsWith(MobileServiceLocalSystemTables.Prefix))
            {
                IDictionary<string, string> existingRecords = null;
                bool analyzeUpserts = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.DetectInsertsAndUpdates);
                bool supportsVersion = false;

                if (analyzeUpserts)
                {
                    MobileServiceSystemProperties systemProperties = await this.settings.GetSystemPropertiesAsync(tableName);
                    supportsVersion = systemProperties.HasFlag(MobileServiceSystemProperties.Version);

                    existingRecords = await GetItemsAsync(tableName, items.Select(i => this.objectReader.GetId(i)), supportsVersion);
                }

                await this.store.UpsertAsync(tableName, items, ignoreMissingColumns);

                foreach (var item in items)
                {
                    string itemId = this.objectReader.GetId(item);
                    LocalStoreOperationKind operationKind = LocalStoreOperationKind.Upsert;

                    if (analyzeUpserts)
                    {
                        if (existingRecords.ContainsKey(itemId))
                        {
                            operationKind = LocalStoreOperationKind.Update;

                            // If the update isn't a result of a local operation, check if the item exposes a version property
                            // and if we truly have a new version (an actual change) before tracking the change. 
                            // This avoids update notifications for records that haven't changed, which would usually happen as a result of a pull
                            // operation, because of the logic used to pull changes.
                            if (this.trackingContext.Source != StoreOperationSource.Local && supportsVersion
                                && string.Compare(existingRecords[itemId], item[MobileServiceSystemColumns.Version].ToString()) == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            operationKind = LocalStoreOperationKind.Insert;
                        }
                    }

                    TrackStoreOperation(tableName, itemId, operationKind);
                }
            }
            else
            {
                await this.store.UpsertAsync(tableName, items, ignoreMissingColumns);
            }
        }

        public Task InitializeAsync()
        {
            return this.store.InitializeAsync();
        }

        private async Task<IDictionary<string, string>> GetItemsAsync(string tableName, IEnumerable<string> ids, bool includeVersion)
        {
            var query = new MobileServiceTableQueryDescription(tableName);
            BinaryOperatorNode idListFilter = ids.Select(t => new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, MobileServiceSystemColumns.Id), new ConstantNode(t)))
                                                 .Aggregate((aggregate, item) => new BinaryOperatorNode(BinaryOperatorKind.Or, aggregate, item));

            query.Filter = idListFilter;
            query.Selection.Add(MobileServiceSystemColumns.Id);

            if (includeVersion)
            {
                query.Selection.Add(MobileServiceSystemColumns.Version);
            }

            QueryResult result = await this.store.QueryAsync(query);

            return result.Values.ToDictionary(t => this.objectReader.GetId((JObject)t), rec => includeVersion ? rec[MobileServiceSystemColumns.Version].ToString() : null);
        }

        private void TrackStoreOperation(string tableName, string itemId, LocalStoreOperationKind operationKind)
        {
            var operation = new StoreOperation(tableName, itemId, operationKind, this.trackingContext.Source, this.trackingContext.BatchId);

            if (this.trackBatches)
            {
               this.operationsBatch.IncrementOperationCount(operationKind)
                   .ContinueWith(t => t.Exception.Handle(e => true), TaskContinuationOptions.OnlyOnFaulted);
            }

            if (this.trackRecordOperations)
            {
                this.eventManager.BackgroundPublish(new StoreOperationCompletedEvent(operation));
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                CompleteBatch();
            }
        }

        private void CompleteBatch()
        {
            if (Interlocked.Exchange(ref this.isBatchCompleted, 1) == 0)
            {
                if (this.trackBatches)
                {
                    this.eventManager.PublishAsync(new StoreOperationsBatchCompletedEvent(this.operationsBatch));
                }
            }
        }
    }
}
