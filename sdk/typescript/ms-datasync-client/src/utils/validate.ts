// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/*
** WARNING: This file explicitly uses type 'any' for checking because
** it's designed to validate arguments in the JavaScript scope.
*/
/*eslint @typescript-eslint/no-explicit-any: [ "off" ]*/

import * as _ from './extensions';

/**
 * Ensures that the provided value is a Date type.
 * 
 * @param value The value to check.
 * @param name The parameter name.
 */
 export function isDate(value: any, name?: string) {
    notNull(value, name);
    if (!_.isDate(value)) {
        throw new TypeError(`${name || 'Value'} is not a Date type.`);
    }
}

/**
 * Ensures that the provided value is an integer.
 * 
 * @param value The value to check.
 * @param name The parameter name.
 */
 export function isInteger(value: any, name?: string) {
    notNull(value, name);
    if (!_.isInteger(value)) {
        throw new TypeError(`${name || 'Value'} is not a number.`);
    }
}

/**
 * Ensures that the provided value is a number.
 * 
 * @param value The value to check.
 * @param name The parameter name.
 */
 export function isNumber(value: any, name?: string) {
    notNull(value, name);
    if (!_.isNumber(value)) {
        throw new TypeError(`${name || 'Value'} is not a number.`);
    }
}

/**
 * Ensures that the provided value is a valid endpoint for a datasync service.
 * 
 * @param value The value to check.
 * @param name The parameter name. 
 */
export function isValidEndpoint(value: URL, name?: string) {
    if (!_.isValidEndpoint(value)) {
        throw new TypeError(`${name || 'endpoint' } "{value.href}" is not valid.`);
    }
}

/**
 * Ensures that the provided value is a valid ID for a datasync service.
 * 
 * @param value The value to check.
 * @param name The parameter name. 
 */
export function isValidId(value: any, name?: string) {
    if (!_.isValidId(value)) {
        throw new TypeError(`${name || 'id'} "{value}" is not valid.`);
    }
}

/**
 * Ensures that the value is not null (or undefined).
 * 
 * @param value The value to check.
 * @param name The parameter name.
 */
export function notNull(value: any, name?: string) {
    if (_.isNull(value)) {
        throw new TypeError(`Parameter ${name || 'Value'} is null`);
    }
}

/** 
 * Ensures that the value is not null, undefined, or empty.
 * 
 * @param value The value to check.
 * @param name The parameter name.
 */
export function notNullOrEmpty(value: any, name?: string) {
    if (_.isNullOrEmpty(value)) {
        throw new TypeError(`Parameter ${name || 'Value'} is null or empty`);
    }
}

/**
 * Ensure that the value is not null, undefined, zero, or empty.
 * 
 * @param value The value to check. 
 * @param name The parameter name.
 */
export function notNullOrZero(value: any, name?: string) {
    if (_.isNullOrZero(value)) {
        throw new TypeError(`Parameter ${name || 'Value'} is null or zero`);
    }
}
