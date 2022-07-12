// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * The options used to configure the DatasyncClient.
 */
export interface DatasyncClientOptions {
    // httpPipeline

    // httpTimeout

    // idGenerator

    /**
     * If set, the installation ID that will be sent to the service with
     * every single request.  If set to the empty string, no installation
     * ID will be sent.
     */
    installationId?: string,

    // offlineStore

    // parallelOperations

    // serializerSettings

    // tableEndpointResolver

    // userAgent
}

const defaultClientOptions: DatasyncClientOptions = {

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
        this._clientOptions = Object.assign({}, defaultClientOptions, options);
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