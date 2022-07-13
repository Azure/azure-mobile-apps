// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * The options for the service client.
 */
export interface ServiceClientOptions {
    /** The timeout for each request, in ms */
    httpTimeout?: number;
}

/**
 * The default client options.
 */
const defaultClientOptions: ServiceClientOptions = {
    httpTimeout: 100000
};

/**
 * An implementation of a filtered HTTP client
 */
export class ServiceClient {
    private readonly _endpoint: URL;
    private readonly _clientOptions: ServiceClientOptions;

    /**
     * 
     * @param endpoint The endpoint to communicate with.
     */
    constructor(endpoint: URL, clientOptions: ServiceClientOptions) {
        this._endpoint = endpoint;
        this._clientOptions = { ...defaultClientOptions, ...clientOptions };
    }
    
    /**
     * The base endpoint being used.
     */
    get endpoint(): URL {
        return this._endpoint;
    }

    /**
     * The client options being used.
     */
    get clientOptions(): ServiceClientOptions {
        return this._clientOptions;
    }
}