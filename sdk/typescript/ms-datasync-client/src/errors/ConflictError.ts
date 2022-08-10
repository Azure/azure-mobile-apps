// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { isError } from '@azure/core-util';
import { ServiceRequest, ServiceResponse } from '../http';

/**
 * An error that is thrown when there is an HTTP error.
 */
 export class ConflictError extends Error {
    /**
     * The request that was made.
     */
    public readonly request?: ServiceRequest;

    /**
     * The response received (if any)
     */
    public readonly response?: ServiceResponse;

    /**
     * Creates a new ConflictError.
     * 
     * @param message The message about why the argument is wrong.
     * @param request The request causing the error.
     * @param response The response causing the error.
     */
    constructor(message: string, request?: ServiceRequest, response?: ServiceResponse) {
        super(message);
        
        this.name = 'ConflictError';
        this.request = request;
        this.response = response;

        Object.setPrototypeOf(this, ConflictError.prototype);
    }

    /**
     * The value of the entity on the server (if available).
     */
    public get serverValue(): unknown | undefined { return this.response?.value; }
}

/**
 * Type guard for ConflictError
 * 
 * @param e - Something caught by a catch clause
 */
export function isConflictError(e: unknown): e is ConflictError {
    if (e instanceof ConflictError) {
        return true;
    }
    return isError(e) && e.name === 'ConflictError';
}
