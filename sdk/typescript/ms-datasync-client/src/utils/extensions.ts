// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { CIDR, createCIDR, parse as parseIPAddress } from 'ip6addr';

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
export function isDate(value: any): boolean {
    return !isNull(value) && (value.name || value.constructor.name) === 'Date';
}

/**
 * Determines if the provided value is an integer.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isInteger(value: any): boolean {
    return isNumber(value) && (parseInt(value, 10) === parseFloat(value));
}

/**
 * Determines if the provided value is in one of the IPv4 CIDR blocks
 * 
 * @param value The value to check.
 * @param cidrs The list of CIDR blocks to check against
 * @returns True if the value matches the check; false otherwise.
 */
 export function isIPinCIDR(value: string, cidrs: Array<string>): boolean {
    const cidrList = cidrs.map<CIDR>((addr: string) => createCIDR(addr));
    return cidrList.some(v => v.contains(value));
}

/**
 * Determines if the provided value is an IPv4 address.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
 export function isIPv4(value: any): boolean {
    try {
        const addr = parseIPAddress(value);
        return addr.kind() === 'ipv4';
    } catch {
        return false;
    }
}

/**
 * Determines if the host provided is on a local network (and hence can use http)
 * 
 * @param value the hostname value
 */
 export function isLocalNetwork(value: string): boolean {
    // Standard hostnames
    if (['localhost', '127.0.0.1', '::1'].includes(value) || value.endsWith('.local')) {
        return true;
    }

    // If it is a IPv4 address and one of the standard "local network" ranges
    if (isIPv4(value) && isIPinCIDR(value, [ '10.0.0.0/8', '172.16.0.0/12', '192.168.0.0/16' ])) {
        return true;
    }

    // Anything else is not local
    return false;
}

/**
 * Determines if the provided value is null or undefined.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNull(value: any): boolean {
    return value === null || value === undefined;
}

/**
 * Determines if the provided value is null, undefined, or empty.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNullOrEmpty(value: any): boolean {
    return isNull(value) || value.length === 0;
}

/**
 * Determines if the provided value is null, or a string with only whitespace.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNullOrWhiteSpace(value: any): boolean {
    return isNull(value) || (typeof value === 'string' && value.trim().length === 0);
}

/**
 * Determines if the provided value is null, undefined, zero or the empty string.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNullOrZero(value: any): boolean {
    return isNull(value) || value === 0 || value === '';
}

/**
 * Determines if the provided value is a number.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isNumber(value: any): boolean {
    return !isNull(value) && typeof value === 'number';
}

/**
 * Determines if the provided value is a suitable valid endpoint for a datasync service.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isValidEndpoint(value: URL): boolean {
    // value must be an absolute URI - this is also a part of URL, so no need to check

    // http://localhost is allowed
    if (value.protocol === 'http:' && isLocalNetwork(value.host)) {
        return true;
    }

    // Otherwise, only https is allowed
    if (value.protocol !== 'https:') {
        return false;
    }

    return true;
}

/**
 * Determines if the provided value is a suitable valid ID for a datasync service.
 * 
 * @param value The value to check.
 * @returns True if the value matches the check; false otherwise.
 */
export function isValidId(value: any): boolean {
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
