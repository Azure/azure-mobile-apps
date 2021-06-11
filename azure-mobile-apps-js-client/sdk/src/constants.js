// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = {
    featuresHeaderName: "X-ZUMO-FEATURES",
    features: {
        JsonApiCall: "AJ",               // Custom API call, where the request body is serialized as JSON
        GenericApiCall: "AG",            // Custom API call, where the request body is sent 'as-is'
        AdditionalQueryParameters: "QS", // Table or API call, where the caller passes additional query string parameters
        OptimisticConcurrency: "OC",     // Table update / delete call, using Optimistic Concurrency (If-Match headers)
        TableRefreshCall: "RF",          // Refresh table call
        TableReadRaw: "TR",              // Table reads where the caller uses a raw query string to determine the items to be returned
        TableReadQuery: "TQ",            // Table reads where the caller uses a function / query OM to determine the items to be returned
        OfflineSync: "OL",               // Table operations performed as part of offline sync (push and pull)
        IncrementalPull: "IP"            // Table reads performed as part of an incremental pull 
    },
    apiVersionHeaderName: "ZUMO-API-VERSION",
    apiVersion: "2.0.0",
    table: {
        idPropertyName: "id",
        sysProps: {
            deletedColumnName: "deleted",
            createdAtColumnName: "createdAt",
            updatedAtColumnName: "updatedAt",
            versionColumnName: "version"
        },
        includeDeletedFlag: "__includeDeleted",
        operationTableName: "__operations",
        pulltimeTableName: "__pulltime"
    }
};
