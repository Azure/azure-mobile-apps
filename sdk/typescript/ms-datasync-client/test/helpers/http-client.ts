// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { AccessToken, GetTokenOptions, TokenCredential } from "@azure/core-auth";
import * as msrest from "@azure/core-rest-pipeline";

// Re-export the access token so we get it from the right place.
export { AccessToken };

/**
 * The PipelineResponse, but without the request requirement.
 */
type MockResponse = Omit<msrest.PipelineResponse, "request">;

/**
 * An implementation of the TokenCredential for mocking
 */
export class MockTokenCredential implements TokenCredential {
    /**
     * The access token to provide to any request.
     */
    public accessToken?: AccessToken;

    /**
     * The last options to be provided.
     */
    public options?: GetTokenOptions;

    /**
     * The number of times a token has been requested;
     */
    public requestCount: number;

    /**
     * The last scopes to be provided.
     */
    public scopes?: string | string[];

    /**
     * Creates a new instance of the MockTokenCredential.
     */
    constructor(token?: AccessToken) {
        this.requestCount = 0;
        this.accessToken = token;
    }

    /**
     * Returns the current access token
     * 
     * @param scopes - the list of scopes to use.
     * @param options - the options to use.
     */
    getToken(scopes: string | string[], options?: GetTokenOptions): Promise<AccessToken | null> {
        this.options = options;
        this.scopes = scopes;
        this.requestCount++;
        
        return Promise.resolve(this.accessToken || null);
    }
}

/**
 * A HttpClient implementation for mocking.
 */
export class MockHttpClient implements msrest.HttpClient {
    /**
     * The requests that this client has received.
     */
    public readonly requests: Array<msrest.PipelineRequest> = [];

    /**
     * A list of responses that this client will send.
     */
    private readonly responses: Array<MockResponse> = [];

    /**
     * Adds a response to the list of responses to send.
     * 
     * @param statusCode - the HTTP status code to send.
     * @param body - if provided, the body to send.
     * @param headers - if provided, the list of headers to send.
     * @returns the mock client (for chaining).
     */
    addRestResponse(statusCode: number, body?: string, headers?: msrest.HttpHeaders): MockHttpClient {
        const response: MockResponse = {
            status: statusCode,
            bodyAsText: body,
            headers: headers || msrest.createHttpHeaders()
        };
        this.responses.push(response);
        return this;
    }
    
        /**
     * Adds a response to the list of responses to send.
     * 
     * @param statusCode - the HTTP status code to send.
     * @param body - if provided, the body to send.
     * @param headers - if provided, the list of headers to send (raw form).
     * @returns the mock client (for chaining).
     */
    addResponse(statusCode: number, body?: string, headers?: msrest.RawHttpHeadersInput): MockHttpClient {
        const response: MockResponse = {
            status: statusCode,
            bodyAsText: body,
            headers: msrest.createHttpHeaders(headers)
        };
        this.responses.push(response);
        return this;
    }

    /**
     * Mock implementation of sendRequest
     * 
     * @param request - the request to be sent.
     * @returns a mock response from the queue of responses.
     */
    async sendRequest(request: msrest.PipelineRequest): Promise<msrest.PipelineResponse> {
        this.requests.push(request);

        const r = this.responses.shift();
        if (typeof r !== "undefined") {
            return { request: request, ...r };
        }

        throw new Error(`No responses left in mock handler (received ${this.requests.length} requests)`);
    }
}