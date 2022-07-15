// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

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
 * The list of standard headers.
 */
export class HttpHeaders {
    /** The Content-Type header */
    static ContentType = 'Content-Type';
}

/**
 * The list of standard content types.
 */
export class ContentType {
    /** JSON content */
    static JSON = 'application/json';
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
        if (typeof content !== 'undefined' && !this._headers.has(HttpHeaders.ContentType)) {
            this._headers.set(HttpHeaders.ContentType, ContentType.JSON);
        }
    }

    /**
     * The content to use for the payload of this request.
     */
    get content(): string | undefined { return this._content; }
    set content(value: string | undefined) { 
        this._content = value; 
        if (typeof value !== 'undefined' && !this._headers.has(HttpHeaders.ContentType)) {
            this._headers.set(HttpHeaders.ContentType, ContentType.JSON);
        } else if (typeof value === 'undefined') {
            this._headers.delete(HttpHeaders.ContentType);
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
        this._reasonPhrase = reasonPhrase;
        this._requestMessage = requestMessage;
        this._headers = headers ?? new Map<string, string>();
        this._content = content;
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

