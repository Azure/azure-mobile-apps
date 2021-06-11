// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('./helpers'),
    promises = require('../../utilities/promises'),
    errors = require('../../utilities/errors'),
    errorTypes = require('./errorTypes'),
    log = require('../../logger');

module.exports = function (connection) {
    return function (statement) {
        if(statement.noop)
            return promises.resolved();

        var parameters = {};

        if(statement.parameters) {
            // support being passed mssql style parameters
            if(statement.parameters.constructor === Array)
                statement = {
                    sql: statement.sql,
                    parameters: helpers.mapParameters(statement.parameters),
                    transform: statement.transform
                };

            // SQLite expects the '@' symbol prefix for each parameter
            Object.keys(statement.parameters).forEach(function (parameterName) {
                parameters['@' + parameterName] = statement.parameters[parameterName];
            });
        }

        log.silly('Executing SQL statement ' + statement.sql + ' with parameters ' + JSON.stringify(parameters));

        return promises.create(function (resolve, reject) {
            connection.all(statement.sql, parameters, function (err, rows) {
                try {
                    if(err) {
                        if(errorTypes.isUniqueViolation(err))
                            reject(errors.duplicate('An item with the same ID already exists'));

                        else if(errorTypes.isInvalidDataType(err))
                            reject(errors.badRequest('Invalid data type provided'));

                        else
                            reject(err);
                    } else {
                        resolve(statement.transform ? statement.transform(rows) : rows);
                    }
                } catch(err) {
                    reject(err);
                }
            });
        });
    };
};
