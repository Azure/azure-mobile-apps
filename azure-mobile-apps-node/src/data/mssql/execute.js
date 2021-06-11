// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var mssql = require('mssql'),
    helpers = require('./helpers'),
    promises = require('../../utilities/promises'),
    errors = require('../../utilities/errors'),
    errorCodes = require('./errorCodes'),
    log = require('../../logger'),
    connection, connectionPromise;

module.exports = function (config, statement) {
    if(statement.noop)
        return promises.resolved();

    if (!connectionPromise) {
        connection = new mssql.Connection(config);
        connectionPromise = connection.connect()
            .catch(function (err) {
                connectionPromise = undefined;
                throw err;
            });
    }

    return connectionPromise.then(executeRequest);

    function executeRequest() {
        var request = new mssql.Request(connection);

        request.multiple = statement.multiple;

        if(statement.parameters) statement.parameters.forEach(function (parameter) {
            var type = parameter.type || helpers.getMssqlType(parameter.value);
            if(type)
                request.input(parameter.name, type, parameter.value);
            else
                request.input(parameter.name, parameter.value);
        });

        log.silly('Executing SQL statement ' + statement.sql + ' with parameters ' + JSON.stringify(statement.parameters));

        return request.query(statement.sql)
            .then(function (results) {
                return statement.transform ? statement.transform(results) : results;
            })
            .catch(function (err) {
                if(err.number === errorCodes.UniqueConstraintViolation)
                    throw errors.duplicate('An item with the same ID already exists');

                if(err.number === errorCodes.InvalidDataType)
                    throw errors.badRequest('Invalid data type provided');

                return promises.rejected(err);
            });
    }
};
