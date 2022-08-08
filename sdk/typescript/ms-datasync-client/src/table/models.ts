// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * Definition of the common fields for a data transfer object
 * from the service.
 */
export interface DataTransferObject {
    /** The globally unique ID for the object */
    id: string;

    /** An opaque string uniquely identifying the version */
    version?: string;

    /** The last date/time that the object was updated, with ms precision */
    updatedAt?: Date;

    /** If true, the item is deleted. */
    deleted?: boolean;
}

/**
 * Definition of a page of items returned from the service.
 */
export interface Page<T> {
    /** The items - always returned. */
    items: [T],

    /** The count of items (only if requested) */
    count?: number,

    /** The link to the next page of items (if there are more pages) */
    nextLink?: string
}

/**
 * Definition of a query against a table.
 */
export interface TableQuery {
    /** An OData filter statement for the table. */
    filter?: string;

    /** The list of fields to return. */
    selection?: [string];

    /** The number of items to skip. */
    skip?: number;

    /** The maximum number of items to return. */
    top?: number;

    /** Whether to include the count of items in the result set. */
    includeCount?: boolean;

    /** Whether to include deleted items in the result set. */
    includeDeletedItems?: boolean;
}