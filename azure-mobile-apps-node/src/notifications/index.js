// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
 * @module azure-mobile-apps/src/notifications
 * @description Functions for managing notification installations and the NH client
 */
var NotificationHubService = require('azure-sb').NotificationHubService;

/**
 * Creates an instance of the notifications module specified in the configuration
 * @param  {notificationsConfiguration} configuration The notifications configuration
 * @return An object with members described below
 */
module.exports = function (configuration) {
    var nhClient = createClient();

    return {
        /** Returns an instance of the {@link http://azure.github.io/azure-sdk-for-node/azure-sb/latest/NotificationHubService.html|Notification Hubs Service} */
        getClient: function () { return nhClient; }
    };

    function createClient() {
        if(!configuration)
            return;
        if(configuration.client)
            return configuration.client;
        if(configuration.hubName && configuration.connectionString)
            return new NotificationHubService(configuration.hubName, configuration.connectionString);
        if(configuration.hubName && configuration.endpoint && configuration.sharedAccessKeyName && configuration.sharedAccessKeyValue)
            return new NotificationHubService(configuration.hubName, configuration.endpoint, configuration.sharedAccessKeyName, configuration.sharedAccessKeyValue);
    }
}
