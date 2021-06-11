// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var loader = require('./loader'),
    assert = require('../utilities/assert').argument;

module.exports = {
    import: function (basePath, importDefinition) {
        return function (path) {
            assert(path, 'A path to a configuration file(s) was not specified');
            var definitions = loader.loadPath(path, basePath);
            Object.keys(definitions).forEach(function (name) {
                var definition = definitions[name];

                // ES2015 Module Syntax support
                if (definition && definition.default)
                    definition = definition.default;

                if (definition && definition.name)
                    name = definition.name;

                importDefinition(name, definition);
            });
        }
    },
    setAccess: function (definition, method) {
        var source = method ? definition[method] : definition;
        if(method) {
            applyDefaultValueFor('authorize');
            applyDefaultValueFor('disable');
        }

        var access = source.access;
        if (access) {
            source.authorize = access === 'authenticated';
            source.disable = access === 'disabled';
        }

        function applyDefaultValueFor(property) {
            if (definition[method].hasOwnProperty(property)) {
                // already set on operation, do nothing
                return;
            } else if (definition.hasOwnProperty(property)) {
                // use table default if exists
                definition[method][property] = definition[property]
            } else {
                // if no setting, use false
                definition[method][property] = false;
            }
        }
    }
}
