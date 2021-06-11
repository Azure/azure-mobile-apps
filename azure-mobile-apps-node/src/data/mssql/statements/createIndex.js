// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers');

module.exports = function (tableConfig, columns) {
    var tableName = helpers.formatTableName(tableConfig),
        indexName,
        columnsString;

    if (Array.isArray(columns)) {
        indexName = columns.join(',');
        columnsString = columns.map(function (column) {
            return '[' + column + ']';
        }).join(',');
    } else if (typeof columns === 'object') {
        // support index configuration object in future
        throw new Error('Index configuration of table \'' + tableConfig.name + '\' should be an array containing either strings or arrays of strings.');
    } else {
        indexName = columns;
        columnsString = '[' + columns + ']';
    }

    return {
        sql: 'CREATE INDEX [' + indexName + '] ON ' + tableName + ' (' + columnsString + ')'
    }
}
