// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as coreClient from '@azure/core-client';
import { AbortSignal } from '@azure/abort-controller';
import * as msrest from '@azure/core-rest-pipeline';
import { v4 as uuid } from 'uuid';

import * as validate from './validate';
import { PROTOCOLVERSION, datasyncClientPolicy } from './DatasyncClientPolicy';
import { HttpMethod } from './HttpMethod';
import { ServiceRequest } from './ServiceRequest';
import { ServiceResponse } from './ServiceResponse';
import * as pkg from '../../package.json';
import { ConflictError, RestError } from '../errors';

const packageDetails = `Datasync/${pkg.version}`;

/**
 * Parameters for the service client.
 */
export interface ServiceHttpClientOptions extends coreClient.ServiceClientOptions {
    /** Overrides the protocol version for the client endpoint. */
    apiVersion?: string;

    /** Sets the timeout (in ms) on a per-request basis */
    timeout?: number;
}

const defaults: ServiceHttpClientOptions = {
    requestContentType: 'application/json; charset=utf-8'
};

// Mapping of the status code to the standard reason phrase.
const statusCodeMap: Map<number, string> = new Map([
    [ 300, 'Multiple Choices' ],
    [ 301, 'Moved Permanently' ],
    [ 302, 'Found' ],
    [ 303, 'See Other' ],
    [ 304, 'Not Modified' ],
    [ 305, 'Use Proxy' ],
    [ 307, 'Temporary Redirect' ],
    [ 308, 'Permanent Redirect'],
    [ 400, 'Bad Request' ],
    [ 401, 'Unauthorized' ],
    [ 402, 'Payment Required' ],
    [ 403, 'Forbidden' ],
    [ 404, 'Not Found' ],
    [ 405, 'Method Not Allowed' ],
    [ 406, 'Not Acceptable' ],
    [ 407, 'Proxy Authentication Required' ],
    [ 408, 'Request Timeout' ],
    [ 409, 'Conflict' ],
    [ 410, 'Gone' ],
    [ 411, 'Length Required' ],
    [ 412, 'Precondition Failed' ],
    [ 413, 'Payload Too Large' ],
    [ 414, 'URI Too Long' ],
    [ 415, 'Unsupported Media Type' ],
    [ 416, 'Range Not Satisfiable' ],
    [ 417, 'Expectation Failed' ],
    [ 418, 'I am a teapot'],
    [ 419, 'Misdirected Request' ],
    [ 425, 'Too Early' ],
    [ 426, 'Upgrade Required' ],
    [ 428, 'Precondition Required' ],
    [ 429, 'Too Many Requests' ],
    [ 431, 'Request Header Fields Too Large' ],
    [ 451, 'Unavailable for legal reasons' ],
    [ 500, 'Internal server error' ],
    [ 501, 'Not implemented' ],
    [ 502, 'Bad gateway' ],
    [ 503, 'Service Unavailable' ],
    [ 504, 'Gateway Timeout' ],
    [ 505, 'HTTP version not supported' ],
    [ 506, 'Variant Also Negotiates' ],
    [ 511, 'Network authentication required' ]
]);

/**
 * The internal service client used to do final communication with the datasync service.
 */
export class ServiceHttpClient extends coreClient.ServiceClient {
    private _apiVersion: string;
    private _serviceEndpoint: URL;
    private _timeout: number;
    private _options: ServiceHttpClientOptions;

    // Mapping of HttpMethod to HttpMethods for @azure/core-client snedRequest.
    private _methodMap: Map<HttpMethod, msrest.HttpMethods> = new Map([
        [ HttpMethod.DELETE, 'DELETE' ],
        [ HttpMethod.POST, 'POST' ],
        [ HttpMethod.PUT, 'PUT' ]
    ]);

    /**
     * Creates a new service client.
     * 
     * @param endpoint the base URI of the datasync service.
     * @param options Any service options to use.
     */
    public constructor(endpoint: string | URL, options: ServiceHttpClientOptions = {}) {
        const baseUri = validate.isAbsoluteHttpEndpoint(endpoint, 'endpoint');
        const apiVersion = options.apiVersion || defaults.apiVersion; 

        const userAgentPrefix = options.userAgentOptions?.userAgentPrefix
            ? `${options.userAgentOptions.userAgentPrefix} ${packageDetails}`
            : packageDetails;

        // The policies list is made up of "additionalPolicies" then the client
        // policies
        const policies = options.additionalPolicies || [];
        const clientPolicy = datasyncClientPolicy({ apiVersion });
        policies.push({ policy: clientPolicy, position: 'perRetry' });

        const coreOptions = {
            ...defaults,
            ...options,
            userAgentOptions: {
                userAgentPrefix
            },
            additionalPolicies: policies,
            requestContentType: 'application/json; charset=utf-8',
            endpoint: baseUri.href
        };

        super(coreOptions);

        this._serviceEndpoint = baseUri;
        this._apiVersion = apiVersion || PROTOCOLVERSION;
        this._timeout = options.timeout || 60000; // 60 seconds
        this._options = { ...coreOptions };
    }

    /**
     * Returns the URL that is being used as the base URI for the datasync service.
     */
    public get endpoint(): URL { return this._serviceEndpoint; }

    /**
     * Returns the API version that is being used for the datasync service.
     */
    public get apiVersion(): string { return this._apiVersion; }

    /**
     * Gets the effective options in use.
     */
    public get options(): ServiceHttpClientOptions { return this._options; }

    /**
     * Sends a request to the remote service.
     * @param request The ServiceRequest object that describes the request.
     * @param abortSignal An AbortSignal to use in aborting the request.
     * @returns A promise that resolves to the ServiceResponse when complete.
     * @throws InvalidResponseError if the response required a payload and didn't receive one.
     */
    public async sendServiceRequest(request: ServiceRequest, abortSignal?: AbortSignal): Promise<ServiceResponse> {
        const url = (request.path.startsWith('http:') || request.path.startsWith('https:')) ? new URL(request.path) : new URL(request.path, this._serviceEndpoint);
        const method = this._methodMap.get(request.method) ?? 'GET';
        const req: msrest.PipelineRequest = {
            abortSignal: abortSignal,
            allowInsecureConnection: url.protocol === 'http:',
            body: () => request.content,
            headers: msrest.createHttpHeaders(request.headers),
            method: method,
            requestId: uuid(),
            timeout: this._timeout,
            url: url.href,
            withCredentials: false
        };
        
        const resp = await this.sendRequest(req);
        const response = new ServiceResponse(resp);
        if (request.ensureResponseContent && response.isSuccessStatusCode && !response.hasContent) {
            throw new RestError('Expected content but non provided', {
                code: 'NO_CONTENT',
                statusCode: response.statusCode,
                request: req,
                response: resp
            });
        }

        if (response.isConflictStatusCode && response.hasContent) {
            throw new ConflictError('Conflict', request, response);
        }

        if (!response.isSuccessStatusCode) {
            throw new RestError(statusCodeMap.get(resp.status) || `HTTP Status ${resp.status}`, {
                code: 'HTTP_ERROR',
                statusCode: response.statusCode,
                request: req,
                response: resp
            });
        }

        return response;
    }
}