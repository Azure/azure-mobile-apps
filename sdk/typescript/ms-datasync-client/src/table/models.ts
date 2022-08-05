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

export interface TableQuery {
    filter?: string;
    selection?: [string];
    skip?: number;
    top?: number;
    includeCount?: boolean;
    includeDeletedItems?: boolean;
}