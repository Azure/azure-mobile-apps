// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ArgumentError } from '../errors';
import { CIDR, createCIDR, parse as parseIPAddress } from 'ip6addr';

/**
 * Returns true if the value contains any character from the character set.
 * 
 * @param value The value to check.
 * @param characterSet The set of characters that are disallowed
 */
function containsCharacter(value: string, characterSet: string): boolean {
    return characterSet.split('').some(char => value.includes(char));
}

/**
 * Returns true of the value contains any non-printable character.
 * 
 * @param value The value to check
 */
function containsNonPrintableCharacter(value: string): boolean {
    return value.split('').map(char => char.charCodeAt(0)).some(code => !(code >= 32 && code < 127));
}

/**
 * Returns true if the provided IP address is in the CIDR block.
 * 
 * @param value the IP address to check.
 * @param cidrs The list of CIDR blocks to check against
 * @returns true if the value matches the check; false otherwise.
 */
export function isIPinCIDR(value: string, cidrs: Array<string>): boolean {
    const cidrList = cidrs.map<CIDR>((addr: string) => createCIDR(addr));
    return cidrList.some(v => v.contains(value));
}

/**
 * Returns true if the provided IP address is an IPv4 address.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
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
 * @param value the hostname value.
 * @returns true if the host is on a local network.
 */
export function isLocalNetwork(value: string): boolean {
    // IPv6 addresses can be ::1 or [::1] - strip off square brackets if needed
    if (value[0] === '[' && value[value.length-1] === ']') {
        value = value.slice(1, value.length - 1);
    }

    // Standard hostnames
    if (['localhost', '127.0.0.1', '::1'].includes(value) || value.endsWith('.local')) {
        return true;
    }

    // IPv4 addresses in private namespace
    if (isIPv4(value) && isIPinCIDR(value, ['10.0.0.0/8', '172.16.0.0/12', '192.168.0.0/16'])) {
        return true;
    }

    // Anything else is not local
    return false;
}

/**
 * Validates that the endpoint is valid.  If it is, returns it as a URL (converting the string).
 * 
 * @param endpoint the endpoint to validate.
 * @param parameterName The name of the parameter.
 * @throws ArgumentError if the endpoint is invalid.
 */
export function isAbsoluteHttpEndpoint(endpoint: string | URL, parameterName: string): URL {
    try {
        const value = typeof endpoint === 'string' ? new URL(endpoint) : endpoint;
        if ((value.protocol === 'https:') || (value.protocol === 'http:' && isLocalNetwork(value.host))) {
            // Fix up value, since we don't allow fragments or query strings in the URL
            value.hash = "";
            value.search = "";
            return value;
        }
        throw new ArgumentError('Invalid endpoint URL', parameterName);
    } catch (TypeError) {
        throw new ArgumentError('Invalid endpoint URL', parameterName);
    }

}

/**
 * Validates that the path is a valid HTTP path.  To be valid, it must
 * be a set of segments separated by '/'.  Each segment must be between
 * 1 and 64 characters max and must only contain valid characters.
 * 
 * @param path the path to be validated.
 * @param parameterName the name of the parameter.
 * @throws ArgumentError if the path is invalid.
 */
export function isRelativePath(path: string, parameterName: string): void {
    if (path !== '') {
        if (!path.startsWith('/')) {
            throw new ArgumentError('path must start with a /', parameterName);
        }
        const segments = path.split('/').slice(1);
        for (const segment of segments) {
            if (segment.length < 1 || segment.length > 64) {
                throw new ArgumentError('Each path segment must be between 1 and 64 characters', parameterName);
            }
            if (containsNonPrintableCharacter(segment) || containsCharacter(segment, "\\*?<>:;'\"[]")) {
                throw new ArgumentError('Each segment must not contain invalid characters', parameterName);
            }
        }
    }
}

/**
 * Validates if the provided headerName is a valid HTTP header name.  If it
 * isn't, then an ArgumentError is thrown.
 * 
 * The HTTP header format is defined in Section 3.2 or RFC 7230.  However, we
 * take a somewhat restrictive view of a header by limiting it to 
 * 
 *  ALPHA *( ALPHA | DIGIT | "-" | "_" )
 * 
 * In addition, the header name may not exceed 64 characters and must end with
 * an alphanumeric digit.
 * 
 * @param headerName The name of the header.
 * @param parameterName The name of the parameter.
 * @throws ArgumentError if the header name is invalid.
 */
export function isValidHeaderName(headerName: string, parameterName: string): void {
    if (headerName.length < 2 || headerName.length > 64) {
        throw new ArgumentError('Invalid header name (invalid length)', parameterName);
    }
    if (!headerName.match(/^[A-Za-z][A-Za-z0-9-_]*$/)) {
        throw new ArgumentError('Invalid header name (regexp match)', parameterName);
    }
    if (!headerName.match(/[A-Za-z0-9]$/)) {
        throw new ArgumentError('Invalid header name (invalid last character)', parameterName);
    }
}

/**
 * Validates if the provided headerValue is a valid HTTP header value.  If it
 * isn't, then an ArgumentError is thrown.
 * 
 * The HTTP header format is defined in Section 3.2 or RFC 7230.  However, we
 * take a somewhat restrictive view of a header by limiting it to the set of 
 * printable characters.  In addition, the value must be between 2 and 256 
 * characters.
 * 
 * @param headerValue The value of the header.
 * @param parameterName The name of the parameter.
 * @throws ArgumentError if the header name is invalid.
 */
export function isValidHeaderValue(headerValue: string, parameterName: string): void {
    if (headerValue.length < 1 || headerValue.length > 256) {
        throw new ArgumentError('Invalid header value (invalid length)', parameterName);
    }
    if (containsNonPrintableCharacter(headerValue)) {
        throw new ArgumentError('Invalid header value (non-printable characters)', parameterName);
    }
}