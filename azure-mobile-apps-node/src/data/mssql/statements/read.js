// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var queries = require('../../../query'),
    format = require('azure-odata-sql').format,
    log = require('../../../logger'),
    helpers = require('./helpers');

module.exports = function (source, tableConfig) {
    // translate the queryjs Query object into the odata format that our formatter expects
    var query = queries.toOData(source);

    return helpers.combineStatements(format(query, tableConfig), transformResult);

    function transformResult(results) {
        log.silly('Read query returned ' + results[0].length + ' results');

        var finalResults = helpers.translateVersion(results[0]);

        // if there is more than one result set, total count is the second query
        if(results.length > 1)
            finalResults.totalCount = results[1][0].count;

        if(query.skip >= 0 && query.take >= 0) {
            finalResults.forEach(function (result) {
                delete result.ROW_NUMBER;
            });
        }

        return finalResults;
    }
};
