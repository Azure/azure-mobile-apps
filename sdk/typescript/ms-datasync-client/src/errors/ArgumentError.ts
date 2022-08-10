// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * An error that is thrown when an argument is wrong.
 */
export class ArgumentError extends Error {
    /**
     * The name of the argument causing the error.
     */
    public readonly argumentName: string;

    /**
     * Creates a new ArgumentError.
     * 
     * @param message The message about why the argument is wrong.
     * @param argumentName The name of the argument causing the error.
     */
    constructor(message: string, argumentName: string) {
        super(message);
        this.argumentName = argumentName;
        Object.setPrototypeOf(this, ArgumentError.prototype);
    }
}