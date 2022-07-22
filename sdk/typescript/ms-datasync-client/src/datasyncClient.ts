// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { DatasyncClientOptions } from './datasyncClientOptions';

const defaultClientOptions: DatasyncClientOptions = {
    httpPipeline: [],
    httpTimeout: 100000
};

/**
 * Provides basic access to a Microsoft Datasync service.
 */
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

    /** The client options for the service. */
    public get clientOptions() { return this._clientOptions; }

    /** The endpoint for the service. */
    public get endpoint() { return this._endpoint; }
}