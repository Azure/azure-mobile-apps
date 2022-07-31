// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as coreClient from '@azure/core-client';
import * as validate from './validate';
import { datasyncClientPolicy } from './DatasyncClientPolicy';
import * as pkg from '../../package.json';

const defaults: ServiceHttpClientOptions = {
    apiVersion: '3.0.0',
    requestContentType: 'application/json; charset=utf-8'
};

const packageDetails = `Datasync/${pkg.version}`;

/**
 * Parameters for the service client.
 */
export interface ServiceHttpClientOptions extends coreClient.ServiceClientOptions {
    /** Overrides the protocol version for the client endpoint. */
    apiVersion?: string;
}

/**
 * The internal service client used to do final communication with the datasync service.
 */
export class ServiceHttpClient extends coreClient.ServiceClient {
    private _apiVersion: string;
    private _serviceEndpoint: URL;

    /**
     * Creates a new service client.
     * 
     * @param endpoint the base URI of the datasync service.
     * @param options Any service options to use.
     */
    public constructor(endpoint: string | URL, options?: ServiceHttpClientOptions) {
        const baseUri = validate.isAbsoluteHttpEndpoint(endpoint, 'endpoint');
        const apiVersion = options?.apiVersion || defaults.apiVersion || '3.0.0'; 

        const userAgentPrefix = options?.userAgentOptions?.userAgentPrefix
            ? `${options.userAgentOptions.userAgentPrefix} ${packageDetails}`
            : packageDetails;

        // The policies list is made up of "additionalPolicies" then the client
        // policies
        const policies = options?.additionalPolicies || [];
        const clientPolicy = datasyncClientPolicy({ apiVersion });
        policies.push({ policy: clientPolicy, position: 'perRetry' });

        super({
            ...defaults,
            ...(options || {}),
            userAgentOptions: {
                userAgentPrefix
            },
            additionalPolicies: policies,
            requestContentType: 'application/json; charset=utf-8',
            endpoint: baseUri.href
        });

        this._serviceEndpoint = baseUri;
        this._apiVersion = apiVersion;
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
     * Sends a request to the remote service.
     * @param request The ServiceRequest object that describes the request.
     * @param signal An AbortSignal to use in aborting the request.
     * @returns A promise that resolves to the ServiceResponse when complete.
     * @throws InvalidResponseError if the response required a payload and didn't receive one.
     */
    // public async SendRequestAsync(request: ServiceRequest, signal?: AbortSignal): Promise<ServiceResponse> {
    //     throw 'not implemented';
    // }
}