// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    format = require('azure-odata-sql').format,
    queries = require('../../../query');

module.exports = function (table, query, version) {
    var tableName = helpers.formatTableName(table),
        filterClause = format.filter(queries.toOData(query)),
        sql = "UPDATE " + tableName + " SET deleted = 0 WHERE " + filterClause.sql,
        parameters = Array.prototype.slice.apply(filterClause.parameters);

    if (version) {
        sql += " AND [version] = @version ";
        parameters.push({ name: 'version', value: new Buffer(version, 'base64') });
    }

    sql += "; SELECT @@rowcount AS recordsAffected; SELECT * FROM " + tableName + " WHERE " + filterClause.sql;

    return {
        sql: sql,
        parameters: parameters,
        multiple: true,
        transform: function (results) {
            return helpers.statements.checkConcurrencyAndTranslate(results, undefined, true);
        }
    };
};
