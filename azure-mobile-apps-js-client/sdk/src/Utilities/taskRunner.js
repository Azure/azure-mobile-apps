// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Implements a task runner that runs synchronous / asynchronous tasks one after the other.
 * @private
 */

var Validate = require('./Validate'),
    Platform = require('../Platform');

module.exports = function () {
    
    var queue = [], // queue of pending tasks 
        isBusy; // true if a task is executing
    
    return {
        run: run
    };
    
    /**
     * Runs the specified task asynchronously after all the earlier tasks have completed
     * @param task Function / task to be executed. The task can either be a synchronous function or can return a promise.
     * @returns A promise that is resolved with the value returned by the task. If the task fails the promise is rejected.
     */
    function run(task) {
        return Platform.async(function(callback) {
            // parameter validation
            Validate.isFunction(task);
            Validate.isFunction(callback);
            
            // Add the task to the queue of pending tasks and trigger task processing
            queue.push({
                task: task,
                callback: callback
            });
            processTask();
        })();
    }
    
    /**
     * Asynchronously executes the first pending task
     */
    function processTask() {
        setTimeout(function() {
            if (isBusy || queue.length === 0) {
                return;
            }

            isBusy = true;

            var next = queue.shift(),
                result,
                error;

            Platform.async(function(callback) { // Start a promise chain
                callback();
            })().then(function() {
                return next.task(); // Run the task
            }).then(function(res) { // task completed successfully
                isBusy = false;
                processTask();
                next.callback(null, res); // resolve the promise returned by the run function
            }, function(err) { // task failed
                isBusy = false;
                processTask();
                next.callback(err); // reject the promise returned by the run function
            });
        }, 0);
    }
};
