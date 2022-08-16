// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { PipelineRequest, PipelineResponse } from "@azure/core-rest-pipeline";

/**
 * An error that is thrown when a conflict occurs.
 */
 export class ConflictError extends Error {
    /**
     * The request causing the error.
     */
    public readonly request: PipelineRequest;

    /**
     * The response causing the error.
     */
    public readonly response: PipelineResponse;

    /**
     * The server side value of the entity in conflict.
     */
    public readonly serverValue: unknown;

    /**
     * Creates a new instance of a ConflictError.
     * 
     * @param request - the request causing the error.
     * @param response - the response causing the error.
     * @param serverValue - the deserialized server value.
     */
    constructor(request: PipelineRequest, response: PipelineResponse, serverValue: unknown) {
        super("Conflict");
        this.request = request;
        this.response = response;
        this.serverValue = serverValue;
        Object.setPrototypeOf(this, ConflictError.prototype);
    }
}

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

