// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
Provides utility functions for asserting specific conditions
@module azure-mobile-apps/src/utilities/assert
*/
module.exports = {
    /**
    Validates that the provided value is not null or undefined and throws
    an error with the specified message if it is.
    @param value The value to test
    @param {string} message The message to attach to the error that is raised
    */
    argument: function (value, message) {
        if(value === undefined || value === null)
            throw new Error(message);
    }
}
