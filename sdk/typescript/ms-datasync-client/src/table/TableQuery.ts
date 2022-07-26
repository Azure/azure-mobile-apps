// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * Description of an ordering statement.
 */
export class Ordering {
    private _fieldName: string;
    private _descending: boolean;

    /**
     * Creates a new ordering statement.
     * 
     * @param fieldName The field to be ordered.
     * @param descending If true, order descending.
     */
    constructor(fieldName: string, descending = false) {
        this._fieldName = fieldName;
        this._descending = descending;
    }

    /**
     * Converts the ordering statement to a string.
     * 
     * @returns The ordering statement as an OData string.
     */
    public toString(): string {
        return `${this._fieldName}${this._descending ? ' desc' : ''}`
    }
}

/**
 * Description of a table query.
 */
export class TableQuery {
    private _skip = 0;
    private _take = 0;

    /**
     * The filter to use limiting the items to be returned.
     */
    public filter = '';

    /**
     * The list of fields to be returned by the request.
     */
    public selection: Array<string> = [];

    /**
     * The ordering for the request.
     */
    public order: Array<Ordering> = [];

    /**
     * The number of items to skip.
     */
    public get skip(): number { return this._skip; }
    public set skip(value: number) {
        if (value < 0) {
            throw new RangeError('Skip value must be greater than or equal to zero');
        }
        this._skip = value;
    }

    /**
     * The number of items to take.
     */
    public get take(): number { return this._take; }
    public set take(value: number) {
        if (value < 0) {
            throw new RangeError('Take value must be greater than or equal to zero');
        }
        this._take = value;
    }

    /**
     * If true, include deleted items in the output.
     */
    public includeDeletedItems = false;

    /**
     * If true, include a count in the output.
     */
    public includeCount = false;
}