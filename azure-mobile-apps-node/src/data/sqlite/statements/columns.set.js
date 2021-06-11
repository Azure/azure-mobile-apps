// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var insert = require('./insert');

module.exports = function (table, itemColumns) {
    var statements = itemColumns.map(function (column) {
        return insert({ name: '__types' }, {
            table: table.name,
            name: column.name,
            type: column.type
        })[0];
    });

    statements.unshift({
        sql: "DELETE FROM [__types] WHERE [table] = @table",
        parameters: {
            table: table.name
        }
    });

    return statements;
}
