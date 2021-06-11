// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var execute = require('./execute'),
    promises = require('../../utilities/promises'),
    schemas = require('./schema'),
    errorCodes = require('./errorCodes');

module.exports = function (table) {
    var api = {
        execute: function (config, statement, item, attempt) {
            var schema = schemas(config);            
            attempt = attempt || 1;

            return execute(config, statement)
                .catch(function (err) {
                    return handleError(err).then(function () {
                        return api.execute(table, statement, item, attempt + 1);
                    });
                });

            function handleError(err) {
                if (attempt >= 3)
                    return promises.rejected(err);

                if(err.number === errorCodes.InvalidObjectName)
                    return schema.createTable(table, item);
                if(err.number === errorCodes.InvalidColumnName)
                    return schema.updateSchema(table, item);
                return promises.rejected(err);
            }

        },
        read: function (config, statement) {
            return execute(config, statement)
                .catch(function (err) {
                    // if dynamic schema is enabled and the error is invalid column, it is likely that the schema has not been
                    // updated to include the column. V1 behavior is to return an empty array, maintain this behavior.
                    if((err.number === errorCodes.InvalidColumnName || err.number === errorCodes.InvalidObjectName)) {
                        var result = [];
                        return result;
                    }
                    throw err;
                });
        }
    };

    return api;
};
