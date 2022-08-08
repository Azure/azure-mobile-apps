// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ArgumentError } from '../errors';
import { HttpMethod } from './HttpMethod';
import { HttpHeaders } from './HttpHeaders';
import * as validate from './validate';

/**
 * A request to the remote service.
 */
export class ServiceRequest {
    private _content?: string;
    private _ensureResponseContent = false;
    private _headers: HttpHeaders = {};
    private _method: HttpMethod = HttpMethod.GET;
    private _path = '';
    private _queryString?: string;

    /**
     * Constructs a new ServiceRequest.
     * 
     * @param method The HTTP method to use.
     * @param path The relative path to the query.
     */
    constructor(method?: HttpMethod, path?: string) {
        if (typeof method !== 'undefined') {
            this.withMethod(method);
        }
        if (typeof path !== 'undefined') {
            this.withPath(path);
        }
    }

    /**
     * Returns the content as an encoded string.
     */
    public get content(): string | undefined { return this._content; }

    /**
     * Returns a boolean that specifies that response content is required.
     */
    public get ensureResponseContent(): boolean { return this._ensureResponseContent; }

    /**
     * Gets the HTTP headers to use.
     */
    public get headers(): HttpHeaders { return this._headers; }

    /**
     * Gets the HTTP method to use.
     */
    public get method(): HttpMethod { return this._method; }

    /**
     * Gets the path to the request.
     */
    public get path(): string { return this._path; }

    /**
     * Gets the current query string;
     */
    public get queryString(): string | undefined { return this._queryString; }

    /**
     * Removes the content from the request.
     * 
     * @returns The current request (for chaining). 
     */
    public removeContent(): ServiceRequest {
        this._content = undefined;
        return this;
    }

    /**
     * Removes a header from the HTTP header set.
     * 
     * @param headerName The name of the header.
     * @returns the current request (for chaining).
     * @throws ArgumentError if the header name is invalid.
     */
    public removeHeader(headerName: string): ServiceRequest {
        validate.isValidHeaderName(headerName, 'headerName');
        if (typeof this._headers[headerName] !== 'undefined') {
            delete this._headers[headerName];
        }
        return this;
    }

    /**
     * Removes all the query string data from the request.
     * 
     * @returns The current request (for chaining). 
     */
    public removeQueryString(): ServiceRequest {
        this._queryString = undefined;
        return this;
    }

    /**
     * Sets the flag that requires response content.
     * 
     * @param ensureReponseContent The value of the flag (default: true).
     * @returns the current request (for chaining).
     */
    public requireResponseContent(ensureReponseContent?: boolean): ServiceRequest {
        if (typeof ensureReponseContent === 'undefined') {
            this._ensureResponseContent = true;
        } else {
            this._ensureResponseContent = ensureReponseContent;
        }
        return this;
    }

    /**
     * Sets the path to a URI
     * @param uri the absolute URI.
     * @returns the current request (for chaining).
     */
    public withAbsoluteUrl(uri: string | URL): ServiceRequest {
        const u = validate.isAbsoluteHttpEndpoint(uri, 'uri');
        this._path = u.href;
        return this;
    }

    /**
     * Sets the content to the provided value.  If the content is
     * empty or undefined, it is removed.  If the content is a
     * value, it is either set (string) or serialized (anything
     * else) before addition.
     * 
     * @param content the content.
     * @returns the current request (for chaining).
     * @throws TypeError if the content cannot be serialized.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    public withContent(content: any): ServiceRequest {
        if (typeof content === 'undefined') {
            this._content = undefined;
        } else if (typeof content === 'string') {
            this._content = content;
            return this;
        } else {
            this._content = JSON.stringify(content);
            this._headers['Content-Type'] = 'application/json; charset=utf-8';
        }
        return this;
    }

    /**
     * Adds a single HTTP header to the list of provided headers.
     * 
     * @param headerName the name of the header.
     * @param headerValue the value of the header.
     * @returns the current request (for chaining).
     * @throws ArgumentError if the header name or value is invalid.
     */
    public withHeader(headerName: string, headerValue: string): ServiceRequest {
        validate.isValidHeaderName(headerName, 'headerName');
        validate.isValidHeaderValue(headerValue, 'headerValue');
        this._headers[headerName] = headerValue;
        return this;
    }

    /**
     * Sets the HTTP headers to the provided headers.
     * 
     * @param headers the headers to use.
     * @returns the current request (for chaining).
     * @throws ArgumentError if any of the headers are invalid.
     */
    public withHeaders(headers: HttpHeaders): ServiceRequest {
        this._headers = {};
        Object.entries(headers).forEach(([key, value]) => { this.withHeader(key, value); });
        return this;
    }

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

    /**
     * Sets the relative path to the resource being requested.
     * 
     * @param path the relative path to the resource being requested.
     * @returns the current request (for chaining).
     * @throws ArgumentError if the relative path is invalid.
     */
    public withPath(path: string): ServiceRequest {
        validate.isRelativePath(path, 'path');
        this._path = path;
        return this;
    }

    /**
     * Sets the query string for the request.  The query string is cleared
     * if blank.  If the query string has a '?' at the start, it is stripped.
     * 
     * @param queryString the new query string.
     * @returns the current request (for chaining).
     */
    public withQueryString(queryString: string): ServiceRequest {
        if (queryString.startsWith('?')) {
            queryString = queryString.slice(1);
        }
        if (queryString.trim() === '') {
            this._queryString = undefined;
        } else {
            this._queryString = queryString.trim();
        }
        return this;
    }

    /**
     * Adds a version match header if a version is set.
     * 
     * @param version The version of the object.
     * @returns the current request (for chaining).
     */
    public withVersionHeader(version?: string): ServiceRequest {
        if (typeof version === 'string' && version.length > 0) {
            return this.withHeader('If-Match', `"${version}"`);
        }
        return this;
    }
}