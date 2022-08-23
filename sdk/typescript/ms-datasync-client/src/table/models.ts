// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { AbortSignalLike } from "@azure/abort-controller";

/**
 * Base definition for a model type, representing the common fields for
 * a data transfer object from the service.
 */
export interface DataTransferObject {
    /**
     * The globally unique ID for the object.
     */
    id: string;

    /**
     * An opaque string uniquely identifying the version.
     */
    version?: string;

    /**
     * The last date/time that the object was updated, with ms precision.
     */
    updatedAt?: Date;

    /**
     * If true, the item is marked for deletion.
     */
    deleted?: boolean;
}

/**
 * A page of items returned from the server.
 */
export interface Page<T> {
    /**
     * The list of items in the page
     */
    items: Array<Partial<T>>;

    /**
     * The count of items (only if requested)
     */
    count?: number;

    /**
     * The link to the next page of items (if there are more pages)
     */
    nextLink?: string;
}

/**
 * Common set of operation options for table operations.
 */
export interface TableOperationOptions {
    /**
     * If set, an abort signal to monitor.
     */
    abortSignal?: AbortSignalLike;

    /**
     * If set and the operation would be a conditional operation, force
     * the operation instead.
     */
    force?: boolean;

    /**
     * If set, the timeout for the request (expressed in milliseconds).
     */
    timeout?: number;
}

/**
 * A query against a table.
 */
export interface TableQuery {
    /** An OData filter statement for the table. */
    filter?: string;

    /** The list of fields to return. */
    selection?: Array<string>;

    /** The ordering */
    orderBy?: Array<string>;

    /** The number of items to skip. */
    skip?: number;

    /** The maximum number of items to return. */
    top?: number;

    /** Whether to include the count of items in the result set. */
    includeCount?: boolean;

    /** Whether to include deleted items in the result set. */
    includeDeletedItems?: boolean;
}

/**
 * A reviver method that can be used during deserialization.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type JsonReviver = (propertyName: string, propertyValue: unknown) => any;