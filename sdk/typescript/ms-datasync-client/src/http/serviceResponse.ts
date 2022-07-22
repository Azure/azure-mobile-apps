// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { _, validate } from "../utils";
import { HttpHeaders, HttpResponseMessage } from "./client";

/**
 * Representation of a service response.
 */
export class ServiceResponse {
    private _content: string;
    private _reasonPhrase: string;
    private _statusCode: number;
    private _headers: HttpHeaders;

    constructor(response: HttpResponseMessage) {
        validate.notNull(response, 'response');

        this._content = response.content || '';
        this._reasonPhrase = response.reasonPhrase;
        this._statusCode = response.statusCode;
        this._headers = Object.fromEntries(Object.entries(response.headers).map(([k, v]) => [ k.toLowerCase(), v]));
    }

    /** The JSON content (if any) for the payload. */
    public get content(): string | undefined { return this._content; }

    /** The ETag value for this response. */
    public get etag(): string | undefined {
        const etagHeader = this._headers['etag'];
        if (typeof etagHeader !== 'undefined') {
            return (etagHeader[0] === '"') ? etagHeader.slice(1, etagHeader.length - 1) : etagHeader;
        }
        return undefined;
    }

    /** True if the payload was sent from the service. */
    public get hasContent(): boolean { return !_.isNullOrEmpty(this._content); }

    /** The HTTP headers that are returned by the service. */
    public get headers(): HttpHeaders { return this._headers; }

    /** True if the status code returned indicates a conflict */
    public get isConflictStatusCode(): boolean { return this._statusCode === 409 || this._statusCode === 412; }

    /** True if the status code returned indicates success */
    public get isSuccessStatusCode(): boolean { return this._statusCode >= 200 && this._statusCode < 300; }

    /** The Reason Phrase for the response. */
    public get reasonPhrase(): string { return this._reasonPhrase; }

    /** The status code for the response. */
    public get statusCode(): number { return this._statusCode; }
}