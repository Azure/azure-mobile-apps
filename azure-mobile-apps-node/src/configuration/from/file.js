// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var merge = require('../../utilities/assign'),
    logger = require('../../logger'),
    path = require('path');

module.exports = function (configuration, filePath) {
    filePath = filePath || path.resolve(configuration.basePath, configuration.configFile);
    try {
        return merge(configuration, require(filePath));
    } catch(ex) {
        if(ex.message.indexOf("Cannot find module") === -1)
            logger.error("Error loading configuration file " + filePath, ex);
        return configuration;
    }
};
