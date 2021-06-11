// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var clone = require('../../utilities/assign'),
    logger = require('../../logger'),
    path = require('path');

module.exports = function (configuration) {
    configuration = clone(configuration);

    // settings.json file is located in D:\home\site\diagnostics - site root is at D:\home\site\wwwroot
    // this could probably be more dynamic, an environment variable would exist to point somewhere relative to settings.json
    var basePath = configuration.basePath || './',
        settingsPath = path.resolve(basePath, '../diagnostics/settings.json'),
        levelMappings = {
            'Verbose': 'silly',
            'Information': 'info',
            'Error': 'error',
            'Warning': 'warn'
        };

    try {
        var settings = require(settingsPath);
        // if AzureDriveEnabled is true (i.e. Application Logging (filesystem) is turned on in the portal), this will override previous settings
        if(settings.AzureDriveEnabled && settings.AzureDriveTraceLevel) {
            configuration.logging = configuration.logging || {};
            configuration.logging.level = levelMappings[settings.AzureDriveTraceLevel] || 'warn';
        }
    } catch(ex) {
        if(ex.message.indexOf("Cannot find module") === -1)
            logger.error("Error loading website configuration file " + settingsPath, ex);
    }

    return configuration;
}
