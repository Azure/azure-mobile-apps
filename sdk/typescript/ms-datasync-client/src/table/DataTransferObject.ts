// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * The definition of the fields in the data transfer object.
 */
export abstract class DataTransferObject {
    /**
     * The globally unique ID of the DTO.
     */
    public id?: string;

    /**
     * The version of the DTO.
     */
    public version?: string;

    /**
     * The last date/time the DTO was updated on the server.
     */
    public updatedAt?: Date;

    /**
     * If true, the item is deleted on the server.
     */
    public deleted?: boolean;
}

/**
 * The definition of a page of results.
 */
export class DataTransferPage<T extends DataTransferObject> {
    /**
     * The number of items in the page of items.
     */
    public readonly count?: number;

    /**
     * The list of items in the page of results.
     */
    public readonly items?: Array<T>;

    /**
     * The link to the next set of results (if any)
     */
    public readonly nextLink?: string;
}