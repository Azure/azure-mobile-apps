// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    util = require('util');

module.exports = function (table, item) {
    var tableName = helpers.formatTableName(table),
        columnNames = [],
        valueParams = [],
        parameters = [];

    Object.keys(item).forEach(function (prop) {
        // ignore the property if it is an autoIncrement id
        if ((prop !== 'id' || !table.autoIncrement) && item[prop] !== undefined && item[prop] !== null) {
            columnNames.push(helpers.formatMember(prop));
            valueParams.push('@' + prop);
            parameters.push({ name: prop, value: item[prop], type: helpers.getMssqlType(item[prop], prop === 'id') });
        }
    });

    var sql = columnNames.length > 0
        ? util.format("INSERT INTO %s (%s) VALUES (%s); ", tableName, columnNames.join(','), valueParams.join(','))
        : util.format("INSERT INTO %s DEFAULT VALUES; ", tableName)

    if(table.autoIncrement)
        sql += util.format('SELECT * FROM %s WHERE [id] = SCOPE_IDENTITY()', tableName);
    else
        sql += util.format('SELECT * FROM %s WHERE [id] = @id', tableName);

    function transformResult(results) {
        return helpers.statements.translateVersion(results[0]);
    }

    return {
        sql: sql,
        parameters: parameters,
        transform: transformResult
    };
}
