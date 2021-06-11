// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    format = require('azure-odata-sql').format,
    queries = require('../../../query'),
    mssql = require('mssql');

module.exports = function (table, query, version) {
    var tableName = helpers.formatTableName(table),
        filterClause = format.filter(queries.toOData(query)),
        deleteStmt = "DELETE FROM " + tableName + " WHERE " + filterClause.sql,
        selectStmt = "SELECT * FROM " + tableName + " WHERE " + filterClause.sql + ";",
        parameters = Array.prototype.slice.apply(filterClause.parameters);

    if (table.softDelete)
        deleteStmt = "UPDATE " + tableName + " SET [deleted] = 1 WHERE " + filterClause.sql + " AND [deleted] = 0";

    if (version) {
        deleteStmt += " AND [version] = @version";
        parameters.push({ name: 'version', type: mssql.VarBinary, value: new Buffer(version, 'base64') });
    }

    deleteStmt += ";SELECT @@rowcount AS recordsAffected;";

    // if we are soft deleting, we can select the row back out after deletion and get up to date versions, etc.
    // if not, we have to select the row out before we delete it
    if (table.softDelete)
        deleteStmt += selectStmt;
    else
        deleteStmt = selectStmt + deleteStmt;

    function transformResults(results) {
        // checkConcurrencyAndTranslate expects results in a particular order - swap ordering if we didn't soft delete
        if(!table.softDelete)
            results = [results[1], results[0]];

        return helpers.statements.checkConcurrencyAndTranslate(results);
    }

    return {
        sql: deleteStmt,
        parameters: parameters,
        multiple: true,
        transform: transformResults
    };
};
