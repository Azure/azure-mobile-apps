// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var logger = require('../../logger'),
    promises = require('../../utilities/promises'),
    sources = ['commandLine', 'defaults', 'environment', 'file', 'object', 'settingsJson'];

module.exports = function (configuration) {
    configuration = configuration || {};

    // for each configuration source above, add a function that applies changes to the configuration and returns the api (i.e. fluent)
    // each source module must export a function with the configuration object as the first parameter, others are optional and passed on
    var api = sources.reduce(function (target, name) {
        target[name] = function () {
            var args = Array.prototype.slice.apply(arguments);
            configuration = require('./' + name).apply(undefined, [configuration].concat(args));
            api.configuration = configuration;
            return api;
        };
        return target;
    }, {});

    // call this to access the generated configuration and apply global configuration
    api.apply = function () {
        module.exports.configureGlobals(configuration);
        return configuration;
    };

    return api;
};

// yeh maybe not the best place, should be in parent module, but then we get a circular reference
module.exports.configureGlobals = function (configuration) {
    logger.configure(configuration.logging);
    promises.setConstructor(configuration.promiseConstructor);
};
