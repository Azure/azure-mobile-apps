// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { PipelineResponse } from '@azure/core-rest-pipeline';
import { HttpHeaders } from './HttpHeaders';

/**
 * A representation of the response from a service.
 */
export class ServiceResponse {
    private _statusCode: number;
    private _content?: string;
    private _value?: unknown;
    private _headers: HttpHeaders;

    /**
     * Creates a new ServiceResponse based on the underlying pipeline response.
     * @param response The response from the REST pipeline.
     */
    public constructor(response: PipelineResponse) {
        this._statusCode = response.status;
        if (typeof response.bodyAsText === 'string' && response.bodyAsText.length > 0) {
            try {
                this._content = response.bodyAsText;
                this._value = JSON.parse(this._content, (field: string, value: string) => {
                    if (field === 'updatedAt') {
                        return new Date(value);
                    } else {
                        return value;
                    }
                });
            } catch {
                // Swallow the error here - we deal with it elsewhere.
            }
        }
        this._headers = {};
        for (const [k, v] of Object.entries(response.headers.toJSON())) {
            this._headers[k.toLowerCase()] = v;
        }
    }

    /**
     * Returns the JSON content for the response.
     */
    get content(): string | undefined { return this._content; }

    /**
     * If provided, the ETag for the response.
     */
    get etag(): string | undefined { return this._headers['etag']?.replace(/^"(.+(?="$))"$/, '$1'); }

    /**
     * Returns true if the response has content.
     */
    get hasContent(): boolean { return typeof this._content === 'string' && this._content.length > 0; }

    /** 
     * Returns true if the response has a value.
     */
    get hasValue(): boolean { return this.hasContent && typeof this._value !== 'undefined'; }

    /**
     * Returns the map of HTTP headers received.
     */
    get headers(): HttpHeaders { return this._headers; }

    /**
     * Returns true if the status code indicates a conflict (body will be the server object).
     */
    get isConflictStatusCode(): boolean { return this._statusCode == 409 || this._statusCode == 412; }

    /**
     * Returns true if the status code indicates success.
     */
    get isSuccessStatusCode(): boolean { return this._statusCode >= 200 && this._statusCode < 299; }

    /**
     * Returns the status code received in the response.
     */
    get statusCode(): number { return this._statusCode; }

    /**
     * Returns the value (if any)
     */
    get value(): unknown { return this._value; }
}