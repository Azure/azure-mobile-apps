// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { HttpHeaders, HttpMethod } from "./client";

export class ServiceRequest {
    /** The HTTP method to use to request the resource. */
    public method: HttpMethod = HttpMethod.Get;

    /** The JSON content to send as the payload. */
    public content?: string;

    /** If true, ensure that the response has content */
    public ensureResponseContent = false;

    /** The additional headers to send with the request. */
    public headers: HttpHeaders = {};

    /** The URI path and query of the resource to request (relative to the base endpoint) */
    public pathAndQuery = '';

    /**
     * Fluent method to requires the response to have content.
     * @returns the current service request.
     */
    public requireResponseContent(): ServiceRequest {
        this.ensureResponseContent = true;
        return this;
    }

    /**
     * Fluent method to set the content on this request.
     * 
     * @param content The content.
     * @returns the current service request.
     */
    public withContent(content: string): ServiceRequest {
        this.content = content;
        return this;
    }

    /**
     * Fluent method to set the headers on this request.
     * 
     * @param headers the HTTP headers.
     * @returns the current service request.
     */
    public withHeaders(headers: HttpHeaders): ServiceRequest {
        this.headers = headers;
        return this;
    }

    /**
     * Fluent method to set the HTTP Method on this request.
     * 
     * @param method the HTTP method.
     * @returns the current service request.
     */
     public withMethod(method: HttpMethod): ServiceRequest {
        this.method = method;
        return this;
    }

    /**
     * Fluent method to set the path and query string on this request.
     * 
     * @param pathAndQuery the Path and Query string.
     * @returns the current service request.
     */
    public withPathAndQuery(pathAndQuery: string): ServiceRequest {
        this.pathAndQuery = pathAndQuery;
        return this;
    }
}