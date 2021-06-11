// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var helpers = require('../helpers');

module.exports = function (existingColumns, table, item) {
    var columns = [],
        added = {};

    item = item || {};
    table = table || {};
    existingColumns = existingColumns || [];

    // map out columns from item
    var itemColumns = Object.keys(item).reduce(function (columns, column) {
        if(item[column] !== undefined && item[column] !== null)
            columns.push({ name: column, type: helpers.getColumnTypeFromValue(item[column]) });
        return columns;
    }, []);

    // these are in order of precedence
    addAutoIncrementId();
    addFromArray(existingColumns);
    addFromObject(table.columns);
    addFromArray(itemColumns);
    addFromObject(reservedColumns());

    return columns;

    function addAutoIncrementId() {
        if(table.autoIncrement) {
            columns.push({ name: 'id', type: 'number' });
            added.id = true;
        }
    }

    function addFromArray(source) {
        source.forEach(function (column) {
            if(!added[column.name])
                columns.push(column);
            added[column.name] = true;
        });
    }

    function addFromObject(source) {
        if(source)
            Object.keys(source).forEach(function (sourceColumn) {
                if(!added[sourceColumn])
                    columns.push({ name: sourceColumn, type: source[sourceColumn] });
                added[sourceColumn] = true;
            });
    }

    function reservedColumns() {
        var columns = {
            id: table.autoIncrement ? 'number' : 'string',
            createdAt: 'date',
            updatedAt: 'date',
            version: 'string'
        };
        if(table.softDelete) columns.deleted = 'boolean';
        return columns;
    }
}
