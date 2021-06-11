// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    assign = require('../../../utilities/assign');

module.exports = function (tableConfig, existingColumns, item) {
    var tableName = helpers.formatTableName(tableConfig),
        columns = assign(itemColumnsSql(), predefinedColumnsSql(), systemPropertiesSql()),
        newColumns = newColumnSql();

    if(newColumns.length > 0)
        return {
            sql: "ALTER TABLE " + tableName + " ADD " + newColumns.join(',')
        };
    else
        return { noop: true };

    function newColumnSql() {
        return Object.keys(columns).reduce(function (sql, property) {
            if(!existingColumns.some(function (column) { return column.name.toLowerCase() === property })) {
                existingColumns.push({ name: property });
                sql.push(columns[property]);
            }
            return sql;
        }, []);
    }

    function systemPropertiesSql() {
        var columns = helpers.getSystemPropertiesDDL(tableConfig.softDelete);
        return Object.keys(columns).reduce(function (sql, column) {
            sql[column.toLowerCase()] = columns[column];
            return sql;
        }, {});
    }

    function itemColumnsSql() {
        if(!item)
            return {};

        return Object.keys(item).reduce(function (sql, property) {
            if(item[property] !== null && item[property] !== undefined && property !== 'id' && !helpers.isSystemProperty(property))
                sql[property.toLowerCase()] = '[' + property + '] ' + helpers.getSqlType(item[property]) + ' NULL';

            return sql;
        }, {});
    }

    function predefinedColumnsSql() {
        if(!tableConfig.columns) return {};
        return Object.keys(tableConfig.columns).reduce(function (sql, columnName) {
            sql[columnName.toLowerCase()] = '[' + columnName + '] ' + helpers.getPredefinedColumnType(tableConfig.columns[columnName]) + ' NULL';
            return sql;
        }, {});
    }
};
