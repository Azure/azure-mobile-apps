// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceRequest, ServiceResponse } from '../http';

/**
 * An error that is thrown when there is an HTTP error.
 */
 export class HttpError extends Error {
    private _request?: ServiceRequest;
    private _response?: ServiceResponse;

    // You have to extend Error, set the __proto__ to Error, and use
    // Object.setPrototypeOf in order to have a proper custom error
    // type in JavaScript.
    __proto__ = Error;

    /**
     * Creates a new HttpError.
     * 
     * @param message The message about why the argument is wrong.
     * @param request The request causing the error.
     * @param response The response causing the error.
     */
    constructor(message: string, request?: ServiceRequest, response?: ServiceResponse) {
        super(message);
        
        this._request = request;
        this._response = response;

        Object.setPrototypeOf(this, HttpError.prototype);
    }

    /**
     * The request causing the error.
     */
    public get request(): ServiceRequest | undefined { return this._request; }

    /**
     * The response causing the error.
     */
    public get response(): ServiceResponse | undefined { return this._response; }
}

/**
 * An error that is thrown because a requested entity is not available.  This could be
 * 410 "Gone" or 404 "Not Found".
 */
export class EntityNotFoundError extends HttpError { }

/**
 * An error that is thrown because a requested entity is in conflict.  This could be 
 * because of 409 "Conflict" or 412 "Precondition failed"
 */
export class ConflictError extends HttpError 
{ 
    /**
     * The value of the entity on the server (if available).
     */
    public get serverValue(): unknown | undefined { return this.response?.value; }
}