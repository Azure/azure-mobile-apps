// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import fetch, { Headers } from 'cross-fetch';
import {  
    HttpMethod, 
    HttpRequestMessage, 
    HttpResponseMessage 
} from './clientModels';
import {
    HttpClientHandler
} from './clientHandlers';


/**
 * Definition of the client option for the FetchClient
 */
export interface FetchClientOptions {
    defaultHeaders?: HttpHeaders
}

/**
 * An implementation of the HttpClientHandler that uses the WATWG fetch library
 */
export class FetchClientHandler extends HttpClientHandler {
    private readonly _options: FetchClientOptions;

    /**
     * Creates a new FetchClientHandler.
     * 
     * @param clientOptions The options for this client.
     */
    constructor(clientOptions?: FetchClientOptions) {
        super();
        this._options = clientOptions || {};
    }

    /**
     * Converts the request headers into the internal form required by the fetch API.
     * 
     * @param request the request object.
     * @returns The request headers.
     */
    private getRequestHeaders(request: HttpRequestMessage): Headers {
        const requestHeaders = new Headers();

        // Append the default request headers if they are defined.
        if (typeof this._options.defaultHeaders !== 'undefined') {
            for (const key in this._options.defaultHeaders) {
                requestHeaders.append(key, this._options.defaultHeaders[key]);
            }
        }

        // Append the request headers.
        request.headers.forEach((value, key) => { requestHeaders.append(key, value); });

        return requestHeaders;
    }

    /**
     * Converts the response headers into the external form for the rest of the library.
     * 
     * @param response The response object.
     * @returns A map of the headers.
     */
    private getResponseHeaders(response: Response): Map<string, string> {
        const headers = new Map<string, string>();
        response.headers.forEach((value, key) => { headers.set(key, value); });
        return headers;
    }

    /**
     * Sends a request to the remote service, and returns the result.
     * 
     * @param request The HttpRequestMessage to send.
     * @param signal An abort signal to monitor.
     * @returns A promise that resolves to the HttpResponseMessage. 
     */
    public async sendAsync(request: HttpRequestMessage, signal?: AbortSignal): Promise<HttpResponseMessage> {
        const requestOptions: RequestInit = {
            method: HttpMethod[request.method].toUpperCase(),
            headers: this.getRequestHeaders(request),
            body: request.content,
            cache: 'no-cache',
            redirect: 'follow',
            signal: signal
        };
        const fetchResponse = await fetch(request.requestUri.href, requestOptions);
        let body: string | undefined;
        if (fetchResponse.status !== 204) {
            try {
                const buffer = await fetchResponse.arrayBuffer();
                body = Buffer.from(buffer).toString('utf8');
            } catch {
                // Do nothing - body is undefined if we can't read from the body.
            }
        }
        const response = new HttpResponseMessage(
            fetchResponse.status,
            fetchResponse.statusText, 
            request, 
            this.getResponseHeaders(fetchResponse),
            body
        );
        return response;
    }
}