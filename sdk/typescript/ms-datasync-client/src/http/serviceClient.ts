// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as client from './client';
import { _, validate } from '../utils';
import { createPipeline, getUserAgent } from './utils';
import { DatasyncClientOptions } from '../datasyncClientOptions';

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
     * 
     * @param endpoint 
     * @param clientOptions 
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
}