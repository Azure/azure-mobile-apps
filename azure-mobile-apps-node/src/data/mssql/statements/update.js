// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers'),
    format = require('azure-odata-sql').format,
    queries = require('../../../query'),
    mssql = require('mssql'),
    util = require('util');

module.exports = function (table, item, query) {
    var tableName = helpers.formatTableName(table),
        setStatements = [],
        versionValue,
        parameters = [],
        filter = filterClause();

    for (var prop in item) {
        if(item.hasOwnProperty(prop)) {
            var value = item[prop];

            if (prop.toLowerCase() === 'version') {
                versionValue = value;
            } else if (prop.toLowerCase() !== 'id') {
                setStatements.push(helpers.formatMember(prop) + ' = @' + prop);
                parameters.push({ name: prop, value: value, type: helpers.getMssqlType(value) });
            }
        }
    }

    var sql = util.format("UPDATE %s SET %s WHERE [id] = @id%s", tableName, setStatements.join(','), filter.sql);
    parameters.push({ name: 'id', type: helpers.getMssqlType(item.id, true), value: item.id });
    parameters.push.apply(parameters, filter.parameters);

    if (versionValue) {
        sql += " AND [version] = @version";
        parameters.push({ name: 'version', type: mssql.VarBinary, value: new Buffer(versionValue, 'base64') });
    }

    sql += util.format("; SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM %s WHERE [id] = @id%s", tableName, filter.sql);

    return {
        sql: sql,
        parameters: parameters,
        multiple: true,
        transform: function (results) {
            return helpers.statements.checkConcurrencyAndTranslate(results, item);
        }
    };

    function filterClause() {
        if(!query)
            return { sql: '', parameters: [] };

        var filter = format.filter(queries.toOData(query), 'q');
        if(filter.sql)
            filter.sql = ' AND ' + filter.sql;

        return filter;
    }
};
