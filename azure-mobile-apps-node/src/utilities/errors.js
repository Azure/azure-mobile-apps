// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/errors
@description Provides utility functions for generating errors with extra properties set
*/
var util = require('util');

function addFactory(target, type) {
    target[type] = function(message) {
        var error = new Error(util.format.apply(null, arguments));
        error[type] = true;
        return error;
    };
    return target;
}

module.exports = ['badRequest', 'concurrency', 'duplicate', 'notFound'].reduce(addFactory, {});

/**
@function badRequest
@description Creates an Error object with the specified message and a badRequest property set to true
@param message {string} The message to attach to the Error object
*/

/**
@function concurrency
@description Creates an Error object with the specified message and a concurrency property set to true
@param message {string} The message to attach to the Error object
*/

/**
@function duplicate
@description Creates an Error object with the specified message and a duplicate property set to true
@param message {string} The message to attach to the Error object
*/

/**
@function notFound
@description Creates an Error object with the specified message and a notFound property set to true
@param message {string} The message to attach to the Error object
*/
