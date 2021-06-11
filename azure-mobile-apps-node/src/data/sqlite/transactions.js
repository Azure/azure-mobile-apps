// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var executeModule = require('./execute'),
    promises = require('../../utilities/promises');

module.exports = function (connection) {
    var execute = executeModule(connection);
    
    return function (statements) {
        return promises.create(function (resolve, reject) {
            var results;
                
            execute({ sql: 'BEGIN TRANSACTION' })
                .then(function () {
                    return promises.series(statements, function (statement) {
                        return execute(statement)
                            .then(function (result) {
                                if(result)
                                    results = result;
                            });
                    });
                })
                .then(function () {
                    execute({ sql: 'COMMIT TRANSACTION' })
                        .then(function () {
                            resolve(results);
                        })
                        .catch(function (err) {
                            reject(err);
                        });
                })
                .catch(function (err) {
                    execute({ sql: 'ROLLBACK TRANSACTION' })
                        .then(function () {
                            reject(err);
                        })
                        .catch(function () {
                            reject(err);
                        });
                });
        });
    };
};
