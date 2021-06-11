// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/object
@description Provides utilities for manipulating Javascript objects
*/
var types = require('./types');

module.exports = {
    /** Returns an array with the values from the source object
    @param source {object} The object to extract values from
    */
    values: function (source) {
        return Object.keys(source).map(function (property) {
            return source[property];
        });
    },

    // if array-like object convert to array
    // {'0': addHeader, '1': return200, authorize: true} should convert to [addHeader,return200]
    convertArrayLike: function (arrayLikeObject) {
        if (types.isObject(arrayLikeObject)) {
            var length = 0;
            while (arrayLikeObject.hasOwnProperty(length))
                length++;
            if (length) {
                arrayLikeObject.length = length;
                arrayLikeObject = Array.prototype.slice.call(arrayLikeObject);
            }
        }
        return arrayLikeObject;
    }
}
