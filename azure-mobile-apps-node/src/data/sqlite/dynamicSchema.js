// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var promises = require('../../utilities/promises'),
    schemaModule = require('./schema'),
    errorTypes = require('./errorTypes');

module.exports = function (connection, table, serialize) {
    var schema = schemaModule(connection, serialize);

    var api = {
        execute: function (statements, item, attempt) {
            attempt = attempt || 1;

            return serialize(statements)
                .catch(function (err) {
                    return handleError(err).then(function () {
                        return api.execute(statements, item, attempt + 1);
                    });
                });

            function handleError(err) {
                if (attempt >= 3)
                    return promises.rejected(err);

                if(errorTypes.isMissingTable(err))
                    return schema.createTable(table, item);
                if(errorTypes.isMissingColumn(err))
                    return schema.updateSchema(table, item);

                return promises.rejected(err);
            }
        },
        read: function (statement) {
            return serialize(statement)
                .catch(function (err) {
                    // if dynamic schema is enabled and the error is invalid column, it is likely that the schema has not been
                    // updated to include the column. V1 behavior is to return an empty array, maintain this behavior.
                    if(errorTypes.isMissingTable(err) || errorTypes.isMissingColumn(err)) {
                        var result = [];
                        return result;
                    }
                    throw err;
                });
        }
    };

    return api;
};
