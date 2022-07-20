// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { isNullOrEmpty } from "../../utils/extensions";

/**
 * The set of HTTP methods that we can use.
 */
export enum HttpMethod {
    Delete,
    Get,
    Patch,
    Post,
    Put
}

/**
 * A type that defines a set of headers.
 */
export interface HttpHeaders {
    [key: string]: string;
}

/**
 * A custom error for the HTTP substack.
 */
export class HttpError extends Error {
}

/**
 * The HttpRequestMessage class contains headers, the HTTP verb, and
 * potentially data.
 */
export class HttpRequestMessage {
    private _method: HttpMethod;
    private _requestUri: URL;
    private _headers: Map<string, string>;
    private _content?: string;

    /**
     * Constructs a new HttpRequestMessage.
     * 
     * @param method The HTTP method to use.
     * @param requestUri The request URI to use.
     * @param content The optional payload.
     * @param headers The optional list of headers.
     */
    constructor(method: HttpMethod, requestUri: URL, content?: string, headers?: Map<string, string>) {
        this._method = method;
        this._requestUri = requestUri;
        this._headers = headers || new Map<string, string>();
        this._content = content;
        if (typeof content !== 'undefined') {
            this.setContentTypeToJson();
        }
    }

    /**
     * Helper method that sets the content type header to the right thing.
     */
    private setContentTypeToJson() {
        if (!this._headers.has('Content-Type')) {
            this._headers.set('Content-Type', 'application/json; charset=utf-8');
        }
    }

    /**
     * The content to use for the payload of this request.
     */
    get content(): string | undefined { return this._content; }
    set content(value: string | undefined) { 
        this._content = value; 
        if (typeof value !== 'undefined') {
            this.setContentTypeToJson();
        } else {
            this._headers.delete('Content-Type');
        }
    }

    /**
     * The HTTP Headers for this request.
     */
    get headers(): Map<string, string> { return this._headers; }
    set headers(value: Map<string, string>) { this._headers = value; }

     /**
     * The HTTP VERB to use.
     */
    get method(): HttpMethod { return this._method; }
    set method(value: HttpMethod) { this._method = value; }

    /**
     * The relative path and query string for this request.
     */
    get requestUri(): URL { return this._requestUri; }
    set requestUri(value: URL) { this._requestUri = value; }
}

export class HttpResponseMessage {
    private _content?: string;
    private _headers: Map<string, string>;
    private _reasonPhrase: string;
    private _requestMessage: HttpRequestMessage;
    private _statusCode: number;

    /**
     * Creates a new HttpResponseMessage.
     * 
     * @param statusCode The HTTP Status Code
     * @param reasonPhrase The HTTP reason phrase sent with the status code
     * @param requestMessage The originating request message
     * @param headers The response headers
     */
    constructor(statusCode: number, reasonPhrase: string, requestMessage: HttpRequestMessage, headers?: Map<string, string>, content?: string) {
        this._statusCode = statusCode;
        this._reasonPhrase = isNullOrEmpty(reasonPhrase) ? this.getDefaultReasonPhrase(statusCode) : reasonPhrase;
        this._requestMessage = requestMessage;
        this._headers = headers ?? new Map<string, string>();
        this._content = content;
    }

    /**
     * when the service does not return a reason phrase, we include a default one.
     * 
     * @param statusCode The status code
     * @returns The default reason
     */
    private getDefaultReasonPhrase(statusCode: number): string {
        switch (statusCode) {
            case 200:   return 'OK';
            case 201:   return 'Created';
            case 202:   return 'Accepted';
            case 204:   return 'No Content';
            case 301:   return 'Moved Permanently';
            case 302:   return 'Found';
            case 304:   return 'Not Modified';
            case 307:   return 'Temporary Redirect';
            case 308:   return 'Permanent Redirect';
            case 400:   return 'Bad Request';
            case 401:   return 'Unauthorized';
            case 403:   return 'Forbidden';
            case 404:   return 'Not Found';
            case 405:   return 'Method Not Allowed';
            case 406:   return 'Not Acceptable';
            case 409:   return 'Conflict';
            case 410:   return 'Gone';
            case 412:   return 'Precondition Failed';
            case 413:   return 'Payload Too Large';
            case 414:   return 'URI Too Long';
            case 415:   return 'Unsupported Media Type';
            case 425:   return 'Too Early';
            case 428:   return 'Precondition Required';
            case 429:   return 'Too Many Requests';
            case 451:   return 'Unavailable for Legal Reasons';
            case 500:   return 'Internal Server Error';
            case 501:   return 'Not Implemented';
            case 502:   return 'Bad Gateway';
            case 503:   return 'Service Unavailable';
            case 504:   return 'Gateway Timeout';
            default:    return `Unknown Status ${statusCode}`;
        }
    }

    /**
     * The content to use for the payload of this request.
     */
    get content(): string | undefined { return this._content; }

    /**
     * The HTTP Headers for this request.
     */
    get headers(): Map<string, string> { return this._headers; }

    /**
     * Gets a value that indicates if the HTTP response was a conflict code.
     */
    get isConflictStatusCode(): boolean { return this._statusCode === 409 || this._statusCode === 412; }

     /**
     * Gets a value that indicates if the HTTP response was successful.
     */
    get isSuccessStatusCode(): boolean { return this._statusCode >= 200 && this._statusCode <= 299; }
    
    /**
     * The reason phrase from the status line.
     */
    get reasonPhrase(): string { return this._reasonPhrase; }

    /**
     * The original request message that led to this response.
     */
    get requestMessage(): HttpRequestMessage { return this._requestMessage; }

    /**
     * The HTTP status code.
     */
    get statusCode(): number { return this._statusCode; }
}

