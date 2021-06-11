// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * @file Test helper module
 */

var Platform = require('../../../src/Platform'),
    _ = require('../../../src/Utilities/Extensions');

/**
 * Executes a series of actions and then resolves or rejects the promise
 * @param actions This is an array of actions. Action can be a function which will be executed if the previous
 *                operation in the chain was successful. Otherwise the promise chain will be rejected.
 *                Action can also be an object with two function properties success and fail. As the name suggests,
 *                the functions success or fail are invoked if the previous operation in the chain succeeded
 *                or failed respectively.
 *                Each action can be synchronous or asynchronous.
 */
function runActions(actions) {
    var chain = Platform.async(function(callback) {
        callback();
    })();

    for (var i in actions) {
        chain = runAction(chain, actions[i]);
    }

    return chain;
}

function runAction(chain, action) {
    return chain.then(function(result) {
        if (_.isFunction(action)) {
            return action(result);
        }
        
        if (_.isArray(action)) {
            var self = action[0];
            var func = action[1];

            if (_.isFunction(func)) {
                return func.apply(self, action.slice(2));
            }
        }

        if (_.isObject(action)) {
            if (action.success) {
                return action.success(result);
            } else {
                $assert.fail('Expected failure while running action ' + action);
            }
        }
        
        $assert.fail('Incorrect action definition');
    }, function(error) {
        if (action && action.fail) {
            return action.fail(error);
        } else {
            $assert.fail('Unexpected failure while running action : ' + action);
            $assert.fail(error);
        }
    });
}

module.exports = {
    runActions: runActions
};
