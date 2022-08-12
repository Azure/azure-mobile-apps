// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * An error that is thrown when an argument provided to a method
 * is invalid in the context it is used.
 */
export class InvalidArgumentError extends Error {
    /**
     * The name of the argument that was in error.
     */
    public readonly argumentName: string;

    /**
     * Creates a new InvalidArgumentError object.
     * 
     * @param message - the error message.
     * @param argumentName - the argument name.
     */
    constructor(message: string, argumentName: string) {
        super(message);
        this.argumentName = argumentName;
        Object.setPrototypeOf(this, InvalidArgumentError.prototype);
    }
}