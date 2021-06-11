// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers');

module.exports = function (tableConfig, existingColumns, allColumns) {
    var tableName = helpers.formatTableName(tableConfig),
        newColumns = determineNewColumns();

    if(newColumns.length > 0)
        return newColumns.map(function (column) {
            return {
                sql: "ALTER TABLE " + tableName + " ADD COLUMN " + column + " NULL"
            };
        });
    else
        return { noop: true };

    function determineNewColumns() {
        var existingColumnHash = existingColumns.reduce(function (columns, column) {
            columns[column.name] = column.type;
            return columns;
        }, {});

        return allColumns.reduce(function (columns, column) {
            if(!existingColumnHash[column.name])
                columns.push(helpers.formatMember(column.name) + ' ' + helpers.getSqlTypeFromColumnType(column.type));
            return columns;
        }, []);
    }
};
