// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var _ = require('./Utilities/Extensions');

/**
 * This module is the entry point for the _Azure Mobile Apps Javascript client SDK_. 
 *
 * The SDK can be consumed in multiple ways:
 * - In the form of [Cordova SDK](https://github.com/Azure/azure-mobile-apps-cordova-client) for use in a Cordova app.
 * - As a standalone Javascript bundle for use in web apps.
 * - As an npm package for use in web apps. 
 *
 * When used in the form of either Cordova SDK or a standalone Javascript bundle, the module's exports are added to the 
 * _WindowsAzure_ namespace. Here's an example of how to use it:
 * ```
 * var client = new WindowsAzure.MobileServiceClient('http://azure-mobile-apps-backend-url');
 * var table = client.getTable('mytable');
 * ```
 * 
 * When used as an npm package, here is how to use it:
 * ```
 * var AzureMobileApps = require('azure-mobile-apps-client');
 * var client = new AzureMobileApps.MobileServiceClient('http://azure-mobile-apps-backend-url');
 * var table = client.getTable('mytable');
 * ```
 * 
 * @exports azure-mobile-apps-client
 */
var api = { // Modules that need to be exposed outside the SDK for all targets
    /**
     * @type {MobileServiceClient} 
     */
    MobileServiceClient: require('./MobileServiceClient'),

    /** 
     * @type {QueryJs}
     */
    Query: require('azure-query-js').Query
};

// Target (i.e. Cordova / Browser / etc) specific definitions that need to be exposed outside the SDK
var targetExports = require('./Platform/sdkExports');

// Export shared as well as target specific APIs
for (var i in targetExports) {
    if ( _.isNull(api[i]) ) {
        api[i] = targetExports[i];
    } else {
        throw new Error('Cannot export definition ' + i + ' outside the SDK. Multiple definitions with the same name exist');
    }
}

/** 
 * @type {MobileServiceSqliteStore}
 * @name MobileServiceSqliteStore
 * @description **Note** that this class is available **_only_** as part of the Cordova SDK.
 */

module.exports = api;

