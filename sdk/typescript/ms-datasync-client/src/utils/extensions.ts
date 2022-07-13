// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/*
** WARNING: This file explicitly uses type 'any' for checking because
** it's designed to validate arguments in the JavaScript scope.
*/
/*eslint @typescript-eslint/no-explicit-any: [ "off" ]*/

/**
 * Determines if the provided value is a Date object.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isDate(value: any) {
    return !isNull(value) && (value.name || value.constructor.name) === 'Date';
}

/**
 * Determines if the provided value is an integer.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isInteger(value: any) {
    return isNumber(value) && (parseInt(value, 10) === parseFloat(value));
}

/**
 * Determines if the provided value is null or undefined.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNull(value: any) {
    return value === null || value === undefined;
}

/**
 * Determines if the provided value is null, undefined, or empty.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNullOrEmpty(value: any) {
    return isNull(value) || value.length === 0;
}

/**
 * Determines if the provided value is null, undefined, zero or the empty string.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNullOrZero(value: any) {
    return isNull(value) || value === 0 || value === '';
}

/**
 * Determines if the provided value is a number.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNumber(value: any) {
    return !isNull(value) && typeof value === 'number';
}

/**
 * Determines if the provided value is a suitable valid ID for a datasync service.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isValidId(value: any) {
    if (isNullOrZero(value)) {
        return false;
    }

    if (typeof value === 'string') {
        if (value.length === 0 || value.length > 255 || value.trim().length === 0) {
            return false;
        }

        const idRegexp = /[+"/?`\\]|[\u0000-\u001F]|[\u007F-\u009F]|^\.{1,2}$/; // eslint-disable-line no-control-regex
        if (idRegexp.test(value)) {
            return false;
        }

        return true;
    }

    return false;
}