// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { HttpError, HttpRequestMessage, HttpResponseMessage } from './clientModels';

/**
 * An interface that all the message handlers for HTTP share.  Use
 * either the DelegatingHandler or HttpClientHandler classes as 
 * base implementations instead of this.
 */
export interface HttpMessageHandler {
    /** The type of the handler */
    readonly handlerType: string;
    /**
     * Sends a request to the rest of the pipeline.
     * 
     * @param request the Request to send
     * @param signal an optional AbortSignal to use
     * @returns A promise that resolves to the response message.
     */
    sendAsync(request: HttpRequestMessage, signal?: AbortSignal): Promise<HttpResponseMessage>;
}

/**
 * The base implementation of a DelegatingHandler
 */
export abstract class DelegatingHandler implements HttpMessageHandler {
    private _innerHandler?: HttpMessageHandler;

    /**
     * Creates a new DelegatingHandler with an optional inner handler.
     * @param innerHandler The inner handler.
     */
    constructor(innerHandler?: HttpMessageHandler) {
        this._innerHandler = innerHandler;
    }

    /**
     * Returns the handler type.
     * 
     * @returns the handler type.
     */
    public get handlerType(): string {
        return 'DelegatingHandler';
    }

    /**
     * Sends a request to the rest of the pipeline.
     * 
     * @param request the Request to send
     * @param signal an optional AbortSignal to use
     * @returns A promise that resolves to the response message.
     */
    public sendAsync(request: HttpRequestMessage, signal?: AbortSignal): Promise<HttpResponseMessage> {
        if (typeof this._innerHandler === 'undefined') {
            throw new HttpError('A delegating handler needs an inner handler')
        } else {
            return this._innerHandler.sendAsync(request, signal);
        }
    }
}

/**
 * The base implementation of the HttpClientHandler
 */
export abstract class HttpClientHandler implements HttpMessageHandler {
    /**
     * Returns the handler type.
     * 
     * @returns The handler type.
     */
    public get handlerType(): string {
        return 'HttpClientHandler';
    }

    /**
     * Sends a request to the remote service.  This method must be implemented.
     * 
     * @param request The HttpRequestMessage to send.
     * @param signal An AbortSignal to watch.
     * @returns A promise that resolves to a response message.
     * @throws HttpException if there is any problem with the request.
     */
    abstract sendAsync(request: HttpRequestMessage, signal?: AbortSignal): Promise<HttpResponseMessage>;
}