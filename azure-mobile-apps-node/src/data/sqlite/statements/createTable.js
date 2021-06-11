// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers');

module.exports = function (tableConfig, columns) {
    var tableName = helpers.formatTableName(tableConfig),
        // systemProperties is used to ensure columns are correct type - include the deleted column here
        systemProperties = helpers.getSystemPropertiesDDL(true);

    return {
        sql: 'CREATE TABLE ' + tableName + ' (' + columnSql() + ')'
    };

    function columnSql() {
        return columns.reduce(function (sql, column) {
            if(systemProperties[column.name])
                sql.push(systemProperties[column.name])
            else if (column.name === 'id')
                sql.push('[id] ' + helpers.getSqlTypeFromColumnType(column.type, true) + ' PRIMARY KEY');
            else
                sql.push(helpers.formatMember(column.name) + ' ' + helpers.getSqlTypeFromColumnType(column.type) + ' NULL');
            return sql;
        }, []).join(', ');
    }
};
