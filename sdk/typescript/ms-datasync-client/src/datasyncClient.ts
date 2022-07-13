// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceClientOptions } from './http';

/**
 * The options used to configure the DatasyncClient.
 */
export interface DatasyncClientOptions extends ServiceClientOptions {
    /** The installation ID (if any) to send to the service with every request */
    installationId?: string;
}

const defaultClientOptions: DatasyncClientOptions = {
    // Set up the default client options here.
};

export class DatasyncClient {
    private _endpoint: string;
    private _clientOptions: DatasyncClientOptions;

    /**
     * Creates a new DatasyncClient that connects to the specified endpoint for
     * information transfer.
     * 
     * @param endpoint The endpoint of the datasync service.
     * @param options The clien toptions used to modify any request/response that is sent.
     * @throws UriFormatException if the endpoint is malformed
     */
    constructor(endpoint: string, options?: DatasyncClientOptions) {
        this._endpoint = endpoint;
        this._clientOptions = { ...defaultClientOptions, ...options };
    }

    /**
     * The client options for the service.
     */
    get clientOptions() {
        return this._clientOptions;
    }

    /**
     * The endpoint for the service.
     */
    get endpoint() {
        return this._endpoint;
    }
}