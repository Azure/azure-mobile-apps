// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var queries = require('../../../query'),
    format = require('azure-odata-sql').format,
    convert = require('../convert'),
    log = require('../../../logger'),
    helpers = require('../helpers');

module.exports = function (source, tableConfig) {
    var results;

    // this is not great, affects original object
    tableConfig.flavor = 'sqlite';

    // translate the queryjs Query object into the odata format that our formatter expects
    var query = queries.toOData(source);

    var statements = format(query, tableConfig);
    statements[0].transform = transformResult;
    statements[0].parameters = helpers.mapParameters(statements[0].parameters);

    // if we only got one statement, there is no count query, just return the select
    if(statements.length === 1)
        return statements[0];

    // otherwise, attach the transform to the count query
    statements[1].transform = transformCountQuery;
    return statements;

    function transformResult(rows) {
        log.silly('Read query returned ' + rows.length + ' results');
        results = rows.map(function (row) {
            return convert.item(tableConfig.sqliteColumns, row);
        });
        return results;
    }

    function transformCountQuery(rows) {
        results.totalCount = rows[0].count;
    }
};
