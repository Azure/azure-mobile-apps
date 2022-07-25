// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * The base Node class for all expressions used for analysis and
 * translation by visitors.  It's designed to interop with other 
 * mopdules that create expression trees using object literals
 * with a type tag.
 */
export class Node {
    private _type: string;

    /**
     * Initializes a new instance of the Node class and sets its type
     * tag.
     */
    constructor() {
        this._type = this.constructor.name;
    }

    /**
     * Type tag of the node that allows for easy dispatch in visitors.
     * This is automatically set in the constructor (so it's important
     * to call super() in derived Node classes).
     */
    public get type(): string { return this._type; }
}
