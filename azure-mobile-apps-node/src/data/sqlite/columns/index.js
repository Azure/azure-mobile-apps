// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var combine = require('./combine'),
    statements = require('../statements'),
    convert = require('../convert'),
    promises = require('../../../utilities/promises');

module.exports = function (connection, serialize) {
    var api = {
        for: get,
        set: set,
        applyTo: function (table, items) {
            return get(table).then(function (columns) {
                return items.map(function (item) {
                    return convert.item(columns, item);
                })
            })
        },
        discover: function (table, item) {
            return get(table).then(function (existingColumns) {
                return combine(existingColumns, table, item);
            });
        }
    };
    return api;

    function get(table) {
        if(table.sqliteColumns)
            return promises.resolved(table.sqliteColumns);

        var statement = [{ sql: "SELECT [name], [type] FROM [__types] WHERE [table] = @table", parameters: { table: table.name } }];

        return serialize(statement)
            .then(function (columns) {
                table.sqliteColumns = columns;
                return columns;
            })
            .catch(function (error) {
                // nothing has been inserted for this table, we're going to return an empty array of results later anyway
                return [];
            });
    }

    function set(table, columns) {
        var setStatements = statements.columns.set(table, columns);
        return serialize(setStatements)
            .catch(function (error) {
                return initialize(table).then(function () {
                    // if we fail this time, we're borked
                    return serialize(setStatements);
                });
            })
            .then(function () {
                table.sqliteColumns = columns;
            });
    }

    function initialize(table) {
        return serialize([statements.columns.createTable(table)]);
    }
};
