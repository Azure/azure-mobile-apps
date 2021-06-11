// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/merge
@description Provides utility functions for merging objects together into a single object
*/

module.exports = {
    /** Merges objects into the target
    @param target {object} The object to merge source objects into
    @param source {object} Any number of source objects to merge
    */
    mergeObjects: function (target, source) {
        toObject(target);

        // wrap objects
        target = { wrap: target };
        source = { wrap: (source || {})};

        // merge objects to enable top level merge
        var merged = mapProperties(target, source, assignProperty);

        // return unwrapped merged object
        return merged.wrap;
    },

    /** Determines the properties that would conflict as part of a merge operation
    @param target {object} The object to merge source objects into
    @param source {object} Any number of source objects to merge
    */
    getConflictingProperties: function (target, source) {
        toObject(target);

        // wrap objects
        target = { wrap: target };
        source = { wrap: (source || {})};

        var conflicts = [];

        function trackConflict(target, source, prop, propertiesList) {
            if (target.hasOwnProperty(prop) && target[prop] !== source[prop]) {
                // slice propertiesList to remove wrap property
                conflicts.push(propertiesList.slice(1).join('.'));
            }
        }

        mapProperties(target, source, trackConflict);

        // return tracked conflicts
        return conflicts;
    }
}

function assignProperty(target, source, prop) {
    target[prop] = source[prop];
}

function mapProperties(target, source, assignmentCallback, propertiesList) {
    var to = toObject(target),
        from = source;

    propertiesList = propertiesList || [];

    Object.keys(Object(source)).forEach(function (prop) {
        // keep a record of seen properties
        propertiesList.push(prop);

        // if first is object and second is function, swap to allow for recursive assignment
        if(isObj(to[prop]) && isFunc(from[prop])) {
            var temp = from[prop];
            from[prop] = to[prop];
            to[prop] = temp;
        }

        // if first is object or function and second is object, assign recursively
        if((isObj(to[prop]) || isFunc(to[prop])) && isObj(from[prop]))
            mapProperties(to[prop], from[prop], assignmentCallback, propertiesList);
        else
            assignmentCallback(to, from, prop, propertiesList);

        propertiesList.pop(prop);
    });

    return to;
}

function toObject(val) {
    if (val === null || val === undefined) {
        throw new TypeError('Object.assign cannot be called with null or undefined');
    }

    return Object(val);
}

function isObj(obj) {
    return typeof obj === 'object';
}

function isFunc(obj) {
    return typeof obj === 'function';
}
