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
    public get request(): ServiceRequest { return this._request; }

    /**
     * The response causing the error.
     */
    public get response(): ServiceResponse { return this._response; }
}