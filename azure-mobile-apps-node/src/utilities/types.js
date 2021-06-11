// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/types
@description Provides utility functions for working with Javascript types
*/

var types = module.exports = {
    curry: function (fn) {
        var slice = Array.prototype.slice,
            args = slice.call(arguments, 1);
        return function () {
            return fn.apply(null, args.concat(slice.call(arguments)));
        };
    },

    extend: function (target, members) {
        for (var member in members) {
            if(members.hasOwnProperty(member))
                target[member] = members[member];
        }
        return target;
    },

    defineClass: function (ctor, instanceMembers, classMembers) {
        ctor = ctor || function () { };
        if (instanceMembers) {
            types.extend(ctor.prototype, instanceMembers);
        }
        if (classMembers) {
            types.extend(ctor, classMembers);
        }
        return ctor;
    },

    deriveClass: function (baseClass, ctor, instanceMembers) {
        var basePrototype = baseClass.prototype;
        var prototype = {};
        types.extend(prototype, basePrototype);

        var getPrototype = function (name, fn) {
            return function () {
                var tmp = this._super;
                this._super = basePrototype;
                var ret = fn.apply(this, arguments);
                this._super = tmp;
                return ret;
            };
        };

        if (instanceMembers)
            for (var name in instanceMembers)
                if(instanceMembers.hasOwnProperty(name))
                    // Check if we're overwriting an existing function
                    prototype[name] = typeof instanceMembers[name] === 'function' && typeof basePrototype[name] === 'function'
                        ? getPrototype(name, instanceMembers[name])
                        : instanceMembers[name];

        ctor = ctor ?
            (function (fn) {
                return function () {
                    var tmp = this._super;
                    this._super = basePrototype;
                    var ret = fn.apply(this, arguments);
                    this._super = tmp;
                    return ret;
                };
            })(ctor)
            : function () { };

        ctor.prototype = prototype;
        ctor.prototype.constructor = ctor;
        return ctor;
    },

    classof: function (o) {
        if (o === null) {
            return 'null';
        }
        if (o === undefined) {
            return 'undefined';
        }
        return Object.prototype.toString.call(o).slice(8, -1).toLowerCase();
    },

    isArray: function (o) {
        return types.classof(o) === 'array';
    },

    isObject: function (o) {
        return types.classof(o) === 'object';
    },

    isDate: function (o) {
        return types.classof(o) === 'date';
    },

    isFunction: function (o) {
        return types.classof(o) === 'function';
    },

    isAsyncFunction: function (o) {
        return types.classof(o) === 'asyncfunction';
    },

    isString: function (o) {
        return types.classof(o) === 'string';
    },

    isNumber: function (o) {
        return types.classof(o) === 'number';
    },

    isError: function (o) {
        return types.classof(o) === 'error';
    },

    isGuid: function (value) {
        return types.isString(value) && /[a-fA-F\d]{8}-(?:[a-fA-F\d]{4}-){3}[a-fA-F\d]{12}/.test(value);
    },

    isEmpty: function (obj) {
        if (obj === null || obj === undefined) {
            return true;
        }
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                return false;
            }
        }
        return true;
    }
}
