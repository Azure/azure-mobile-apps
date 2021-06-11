// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    transforms = require('./transforms');

module.exports = function (table) {
    return {
        sql: 'DELETE FROM ' + helpers.formatTableName(table),
        transform: transforms.ignoreResults
    }
}
