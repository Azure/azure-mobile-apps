// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * An error that is thrown when an argument is wrong.
 */
export class ArgumentError extends Error {
    private _name: string;

    // You have to extend Error, set the __proto__ to Error, and use
    // Object.setPrototypeOf in order to have a proper custom error
    // type in JavaScript.
    __proto__ = Error;

    /**
     * Creates a new ArgumentError.
     * 
     * @param message The message about why the argument is wrong.
     * @param argumentName The name of the argument causing the error.
     */
    constructor(message: string, argumentName: string) {
        super(message);
        this._name = argumentName;
        Object.setPrototypeOf(this, ArgumentError.prototype);
    }

    /**
     * The name of the argument causing the error.
     */
    public get argumentName(): string { return this._name; }
}