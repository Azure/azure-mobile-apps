// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * The list of HTTP methods supported.
 */
 export enum HttpMethod {
    /** An item will be deleted. */
    DELETE,
    /** An item will be read. */
    GET,
    /** An item will be created. */
    POST,
    /** An item will be replaced. */
    PUT,
    /** A list of items will be read. */
    QUERY
}