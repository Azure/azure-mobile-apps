// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ArgumentError } from '../errors';
import { HttpMethod } from './HttpMethod';

/**
 * A request to the remote service.
 */
export class ServiceRequest {
    /** The HTTP method to use. */
    private _method: HttpMethod = HttpMethod.GET;

    /**
     * Constructs a new ServiceRequest.
     * 
     * @param method The HTTP method to use.
     */
    constructor(method?: HttpMethod) {
        if (typeof method !== 'undefined') {
            this.withMethod(method);
        }
    }

    /**
     * Gets the HTTP method to use.
     */
    public get method(): HttpMethod { return this._method; }

    /**
     * Sets the method for the request.
     * 
     * @param method the method for the request.
     * @returns the current request (for chaining).
     * @throws ArgumentError if the method is an invalid method.
     */
    public withMethod(method: HttpMethod): ServiceRequest {
        if (method in HttpMethod) {
            this._method = method;
        } else {
            throw new ArgumentError('Invalid value', 'method');
        }
        return this;
    }
}