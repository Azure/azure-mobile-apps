// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = function (configuration) {
    return function (table) {
        return {
            name: table.name,
            description: 'Operations for the ' + table.name + ' table'
        }
    };
};
