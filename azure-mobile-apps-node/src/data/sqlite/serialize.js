// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var transactions = require('./transactions'),
    promises = require('../../utilities/promises');

module.exports = function (connection) {
    var transaction = transactions(connection),
        queue = [],
        current;
    
    return function (statements) {
        if(statements.constructor !== Array)
            statements = [statements];
            
        var deferred = createDeferred(statements);
        queue.push(deferred);
        executeNext();
        return deferred.promise;
    }
    
    function executeNext() {
        if(!current && queue.length > 0) {
            current = queue.shift();
            current.execute();
        }
    }
    
    function createDeferred(statements) {
        // we're creating a "proxy" for the actual promise that we can execute after the previous statement completes
        var deferred = {},
            promise = promises.create(function (resolve, reject) {
                deferred.resolve = resolve;
                deferred.reject = reject;
            });

        deferred.promise = promise;            
        deferred.execute = function () {
            transaction(statements)
                .then(deferred.resolve)
                .catch(deferred.reject)
                .then(function () {
                    current = undefined;
                    executeNext();
                });
        };
        
        return deferred;
    }
}