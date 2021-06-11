// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
The azure-mobile-apps module is the Nodejs implementation of Azure Mobile Apps
@module azure-mobile-apps
@see {@link http://azure.microsoft.com/en-us/services/app-service/mobile/ Azure Mobile Apps}
*/

var loadConfiguration = require('./configuration'),
    table = require('./express/tables/table'),
    logger = require('./logger'),
    promises = require('./utilities/promises'),
    query = require('./query'),

    platforms = {
        express: require('./express'),
    };

/**
Creates an instance of the azure-mobile-apps server object for the platform specified in the configuration.
The top level exported function creates an instance ready for local debugging and Azure hosted configurations.
Express 4.x is currently the only supported platform.
@param {configuration} configuration Top level configuration for all aspects of the mobile app
@param {object} environment=process.env An object containing the environment to load configuration from
@returns {module:azure-mobile-apps/src/express}
*/
module.exports = function (configuration, environment) {
    return module.exports.create(loadConfiguration.from()
        .defaults(configuration)
        .environment(environment)
        .settingsJson()
        .file()
        .object(configuration)
        .commandLine()
        .apply());
};

module.exports.create = function (configuration) {
    return platforms[configuration.platform](configuration);
};

/**
@function table
@description Creates an instance of a table definition object
@returns {module:azure-mobile-apps/src/express/tables/table}
*/
module.exports.table = table;

/** @type {module:azure-mobile-apps/src/logger} */
module.exports.logger = logger;

/** @type {module:azure-mobile-apps/src/query} */
module.exports.query = query;

/** @type {module:azure-mobile-apps/src/utilities/promises} */
module.exports.promises = promises;

// this is purely a helper function to allow intellisense for custom API definitions
module.exports.api = function (definition) { return definition; };
