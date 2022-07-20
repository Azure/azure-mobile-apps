// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceClientOptions } from './models';
import { HttpMessageHandler, HttpHeaders } from './client';
import { _, uuid, validate } from '../utils';
import { version } from '../../package.json';
import { configStore } from '../platform';

function createPipeline(handlers: HttpMessageHandler[]): HttpMessageHandler {

}

/**
 * Returns the installation ID, which is a string that uniquely identifies this app on this device.
 * 
 * @returns The installation ID for this app on this device.
 */
function getInstallationId() {
    return configStore.getOrSet('installationId', uuid());
}

/**
 * Returns the user agent that requests should use.  Note that some platforms do not allow you to
 * specify the user agent, but we have a second header for those cases.
 * 
 * @returns The user agent.
 */
function getUserAgent() {
    const baseVersion = version.split(/\./).slice(0, 2).join('.');
    return `Datasync/${baseVersion} (lang=typescript;ver=${version})`;
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
    protected rootHandler: HttpMessageHandler;

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
    private _options: ServiceClientOptions;

    /**
     * The default headers to add to every single request.
     */
    private _headers: HttpHeaders;

    /**
     * 
     * @param endpoint 
     * @param clientOptions 
     */
    public constructor(endpoint: URL, clientOptions: ServiceClientOptions) {
        validate.isValidEndpoint(endpoint, 'endpoint');

        this._endpoint = endpoint;
        this._options = clientOptions;
        this._installationId = this._options.installationId || getInstallationId();
        this.rootHandler = createPipeline(this._options.httpPipeline || new Array<HttpMessageHandler>());
        this._headers = { 'ZUMO-API-VERSION': ServiceClient.PROTOCOL_VERSION };
        const userAgent = this._options.userAgent || getUserAgent();
        if (!_.isNullOrWhiteSpace(userAgent)) {
            this._headers['X-ZUMO-VERSION'] = userAgent;
        }
        if (!_.isNullOrWhiteSpace(this._installationId)) {
            this._headers['X-ZUMO-INSTALLATION-ID'] = this._installationId;
        }
    }
}