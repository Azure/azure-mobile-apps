// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    transforms = require('./transforms'),
    format = require('azure-odata-sql').format,
    queries = require('../../../query'),
    errors = require('../../../utilities/errors');

module.exports = function (table, query, version) {
    var tableName = helpers.formatTableName(table),
        filterClause = format.filter(queries.toOData(query)),
        result,
        deleteStmt = {
            sql: table.softDelete 
                ? "UPDATE " + tableName + " SET [deleted] = 1 WHERE " + filterClause.sql + " AND [deleted] = 0"
                : "DELETE FROM " + tableName + " WHERE " + filterClause.sql,
            parameters: helpers.mapParameters(filterClause.parameters),
            transform: transforms.ignoreResults
        },
        selectStmt = {
            sql: "SELECT * FROM " + tableName + " WHERE " + filterClause.sql,
            parameters: helpers.mapParameters(filterClause.parameters),
            transform: function (rows) {
                result = transforms.prepareItems(table)(rows);
            }
        },
        countStmt = {
            sql: "SELECT changes() AS recordsAffected",
            transform: function (rows) {
                if(rows[0].recordsAffected === 0) {
                    // we want to 404 if the item didn't exist, was filtered or soft deleted
                    if(!result || result.deleted)
                        throw errors.notFound('No records were updated');

                    var error = errors.concurrency('No records were updated');
                    error.item = result;
                    throw error;
                }
                return result;
            }
        };

    if (version) {
        deleteStmt.sql += " AND [version] = @version";
        deleteStmt.parameters.version = helpers.fromBase64(version);
    }

    // if we are soft deleting, we can select the row back out after deletion and get up to date versions, etc.
    // if not, we have to select the row out before we delete it
    if (table.softDelete)
        return [deleteStmt, selectStmt, countStmt];
    else
        return [selectStmt, deleteStmt, countStmt];
};
