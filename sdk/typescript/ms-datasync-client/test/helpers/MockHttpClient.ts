// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { 
    createHttpHeaders, 
    HttpClient, 
    HttpHeaders, 
    HttpMethods, 
    PipelineRequest, 
    PipelineResponse 
} from '@azure/core-rest-pipeline';
import * as sdk from '../../src/http';

export class MockHttpClient implements HttpClient {
    private _requests: Array<PipelineRequest> = [];
    private _responses: Array<Partial<PipelineResponse>> = [];

    get requests(): Array<PipelineRequest> { return this._requests; }

    /**
     * Converts an object version of headers to a Map version.
     * 
     * @param headers the HTTP headers in object form.
     * @returns The Map form of the HTTP headers.
     */
    getHeaders(headers?: sdk.HttpHeaders): HttpHeaders {
        const result = createHttpHeaders();
        if (typeof headers !== 'undefined') {
            for (const [k, v] of Object.entries(headers)) {
                result.set(k, v);
            }    
        }
        return result;
    }

    createRequest(method: HttpMethods, url: string, headers?: sdk.HttpHeaders, body?: string) {
        const request: PipelineRequest = {
            method: method,
            url: url,
            headers: this.getHeaders(headers),
            timeout: 0,
            withCredentials: false,
            requestId: 'mock'
        };

        if (typeof body !== 'undefined') {
            request.body = () => body;
        }

        return request;
    }

    addResponse(statusCode: number, body?: string, headers?: sdk.HttpHeaders) {
        const response: Partial<PipelineResponse> = {
            status: statusCode,
            bodyAsText: body,
            headers: this.getHeaders(headers)
        };
        this._responses.push(response);
    }

    async sendRequest(request: PipelineRequest): Promise<PipelineResponse> {
        this._requests.push(request);

        if (this._responses.length == 0) {
            throw new Error('No responses left');
        }

        const r = this._responses.shift();
        if (typeof r !== 'undefined') {
            const response: PipelineResponse = {
                request: request,
                status: r.status || 200,
                bodyAsText: r.bodyAsText,
                headers: r.headers || createHttpHeaders()
            }; 
            return response;
        } 

        throw 'No more responses available';
    }
}