// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as client from './client';
import { _, validate } from '../utils';
import { DatasyncClientOptions } from '../datasyncClientOptions';
import { createPipeline, getErrorMessageFromContent, getRequestMessage, getUserAgent } from './utils';
import { ServiceRequest } from './serviceRequest';
import { ServiceResponse } from './serviceResponse';

/**
 * An error thrown by the service client.
 */
export class ServiceClientError extends Error {
    private _request?: client.HttpRequestMessage;
    private _response?: client.HttpResponseMessage;

    /**
     * 
     * @param message The error message.
     * @param request The request object.
     * @param response The response object.
     */
    constructor(message: string, request?: client.HttpRequestMessage, response?: client.HttpResponseMessage) {
        super(message);
        this._request = request;
        this._response = response;
    }

    /** The HTTP request message causing the issue. */
    public get request(): client.HttpRequestMessage | undefined { return this._request; }

    /** The HTTP response message causing the issue. */
    public get response(): client.HttpResponseMessage | undefined { return this._response; }
}

/**
 * An internal version of a http cleint that provides pipeline
 * policies and standardized headers.
 */
export class ServiceClient {
    /**
     * The datasync protocol version that this library implements.
     */
    static PROTOCOL_VERSION = "3.0.0";

    /**
     * The root of the HTTP message handler pipeline.
     */
    protected rootHandler: client.HttpMessageHandler;

    /**
     * The base URL for this client.
     */
    private _endpoint: URL;

    /**
     * The installation ID for this app on this device.
     */
    private _installationId: string;

    /**
     * The client options to use for this client.
     */
    private _options: DatasyncClientOptions;

    /**
     * The default headers to add to every single request.
     */
    private _headers: client.HttpHeaders;

    /**
     * Creates a new ServiceClient.
     * 
     * @param endpoint the absolute URL to the service client endpoint.
     * @param clientOptions The client options to use for modifying the request.
     */
    public constructor(endpoint: URL, clientOptions: DatasyncClientOptions) {
        validate.isValidEndpoint(endpoint, 'endpoint');

        this._endpoint = endpoint;
        this._options = clientOptions;
        this._installationId = this._options.installationId || '';
        this.rootHandler = createPipeline(this._options.httpPipeline || []);
        this._headers = { 'ZUMO-API-VERSION': ServiceClient.PROTOCOL_VERSION };
        const userAgent: string = this._options.userAgent !== '' ? this._options.userAgent || getUserAgent() : '';
        if (!_.isNullOrEmpty(userAgent)) {
            this._headers['X-ZUMO-VERSION'] = userAgent.trim();
        }
        if (!_.isNullOrWhiteSpace(this._installationId)) {
            this._headers['X-ZUMO-INSTALLATION-ID'] = this._installationId.trim();
        }
    }

    /**
     * The base URL for all requests using this service client.
     */
    public get endpoint(): URL { return this._endpoint; }

    /**
     * The unique identifier for this app on this device.
     */
    public get installationId(): string { return this._installationId; }

    /**
     * The default headers to add to the service request.
     */
    public get defaultHeaders(): client.HttpHeaders { return this._headers; }

    /**
     * Sends the request through the HTTP pipeline with additional default headers.  Any
     * headers in the request will override the default headers.
     * 
     * @param request The request to send.
     * @param signal An AbortSignal to monitor.
     * @returns The response from the service.
     */
    public async sendRequestAsync(request: client.HttpRequestMessage, signal?: AbortSignal): Promise<client.HttpResponseMessage> {
        validate.notNull(request, 'request');
        request.headers = { ...this.defaultHeaders, ...request.headers };
        return await this.rootHandler.sendAsync(request, signal);
    }

    /**
     * Sends the request through the HTTP pipeline with the standard headers.
     * 
     * @param serviceRequest The request to send.
     * @param signal An AbortSignal to monitor.
     * @returns The response from the service.
     */
    public async sendAsync(serviceRequest: ServiceRequest, signal?: AbortSignal): Promise<ServiceResponse> {
        validate.notNull(serviceRequest, 'serviceRequest');
        const request = getRequestMessage(serviceRequest, this.endpoint);
        const response = await this.sendRequestAsync(request, signal);
        if (!response.isSuccessStatusCode)  {
            const message = getErrorMessageFromContent(response.content || '') || `The request could not be complete (${response.reasonPhrase})`;
            throw new ServiceClientError(message, request, response);
        }

        if (serviceRequest.ensureResponseContent) {
            if (_.isNullOrEmpty(response.content)) {
                throw new ServiceClientError('The server did not provide a response with the expected content', request, response);
            }
        }

        return new ServiceResponse(response);
    }
}