// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { Addr as IpAddress, CIDR, createCIDR, parse as parseIPAddress } from "ip6addr";

/**
 * Returns true if the provided service URL is a valid datasync service URL.
 * 
 * @param endpointUrl - the endpoint URL to check.
 * @returns true if the endpointUrl is valid; false otherwise.
 */
export function isDatasyncServiceUrl(endpointUrl: URL): boolean {
    if (endpointUrl.search !== "" || endpointUrl.hash !== "") {
        return false;
    }
    return endpointUrl.protocol === "https:" || (endpointUrl.protocol === "http:" && isLocalNetwork(endpointUrl.hostname));
}

/**
 * Returns true if the provided entity ID is a valid ID for the datasync service.
 * 
 * @param id - the entity ID to check.
 * @returns true if the entity ID is valid; false otherwise.
 */
export function isEntityId(id?: unknown): boolean {
    if (typeof id === "string") {
        return new RegExp(/^[a-zA-Z0-9][a-zA-Z0-9-_.]{0,126}$/).test(id);
    }
    return false;
}

/**
 * Returns true if the provided IP address is in the CIDR block.
 * 
 * @param value - the IP address to check.
 * @param cidrs - the list of CIDR blocks to check against.
 * @returns true if the value matches the check; false otherwise.
 */
export function isInCIDR(value: string, cidrs: Array<string>): boolean {
    if (!isIPv4(value)) {
        return false;
    }
    const cidrList = cidrs.map<CIDR>((addr: string) => createCIDR(addr));
    return cidrList.some(v => v.contains(value));
}

/**
 * Returns true if the provided IP address is an IPv4 address.
 * 
 * @param value The IP address to check.
 * @returns true if the value is an IPv4 address; false otherwise.
 */
export function isIPv4(value: string | number | IpAddress): boolean {
    try {
        const addr = parseIPAddress(value);
        return addr.kind() === "ipv4";    
    } catch {
        return false;
    }
}

/**
 * Determines if the host provided is on a local network (and hence can use http protocol)
 * 
 * @param value - The hostname value to check.
 * @returns true if the host is on a local network; false otherwise.
 */
export function isLocalNetwork(value: string): boolean {
    // IPv6 addresses can be ::1 or [::1] - strip off square brackets if needed
    if (value[0] === "[" && value[value.length-1] === "]") {
        value = value.slice(1, value.length - 1);
    }

    // Standard hostnames
    if (["localhost", "127.0.0.1", "::1"].includes(value) || value.endsWith(".local")) {
        return true;
    }

    // IPv4 addresses in private namespace
    if (isIPv4(value) && isInCIDR(value, ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16"])) {
        return true;
    }

    // Anything else is not local
    return false;
}

/**
 * Determines if the provided string is empty or null or undefined.
 * 
 * @param value - the value to check.
 * @returns true if the value contains some text; false otherwise.
 */
export function isNotEmpty(value: string | undefined | null): boolean {
    return (typeof value !== "undefined" && value !== null && value.length > 0);
}

/**
 * Determines if the provided string is a valid table name
 * 
 * @param value - the value to check.
 * @returns true if the value is a valid datasync table name; false otherwise.
 */
export function isTableName(value: string): boolean {
    return new RegExp(/^[a-z][a-z0-9_]{0,63}$/).test(value);
}

