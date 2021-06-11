// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/assign
*/
/**
Recursively assign properties from provided objects. Arguments are processed right to left.
@param {object} source Any number of objects to assign
*/
module.exports = function assign() {
    var result = {},
        args = Array.prototype.slice.call(arguments),
        reduceArrays = typeof args[args.length - 1] == 'function' ? args.pop() : undefined;

    for(var i = 0, l = args.length; i < l; i++) {
        var o = args[i];

        for (var prop in o) {
            if (!o.hasOwnProperty(prop))
                continue;

            if (typeof o[prop] == 'object' && typeOf(o[prop]) == 'object')
                result[prop] = assign(result[prop] || {}, o[prop], reduceArrays);

            else if (reduceArrays && typeOf(o[prop]) == 'array')
                result[prop] = reduceArrays(result[prop] || [], Array.prototype.slice.call(o[prop]));

            else
                result[prop] = o[prop];
        }
    }

    return result;
};

function typeOf(obj) {
    return Object.prototype.toString.call(obj).match(/\[object\s*([^\]]+)\]/)[1].toLowerCase();
}
