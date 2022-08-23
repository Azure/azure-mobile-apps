// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { CommonClientOptions, ServiceClient } from "@azure/core-client";
import { TokenCredential, isTokenCredential } from "@azure/core-auth";
import { bearerTokenAuthenticationPolicy } from "@azure/core-rest-pipeline";

import * as pkg from "../package.json";
import { 
    InvalidArgumentError, 
    datasyncClientPolicy, 
    validate
} from "./utils";
import { DatasyncTable, DataTransferObject, JsonReviver, RemoteTable } from "./table";

/**
 * Client options used to configure the Datasync Client API requests.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface DatasyncClientOptions extends CommonClientOptions {
    /**
     * If set, sends a different API version.  You should probably not set this.
     */
    apiVersion?: string;

    /**
     * The credential scopes, used when authenticating via OAuth2
     */
    scopes?: string | string[];

    /**
     * A resolver for turning a table name into a table path.
     */
    tablePathResolver?: (tableName: string) => string;

    /**
     * A resolver for turning a table name into a JSON reviver.
     */
    tableReviver?: (tableName: string) => JsonReviver | undefined;

    /**
     * The default timeout (in milliseconds) for operations.
     */
    timeout?: number;

    // Any custom options configured at the client level go here.
}

/**
 * The client class used to interact with a Datasync service.
 */
export class DatasyncClient {
    /**
     * The current client options in effect.
     */
    public readonly clientOptions: DatasyncClientOptions;

    /**
     * The authentication credential being used.
     */
     public readonly credential?: TokenCredential;

    /**
     * The base URL for the datasync service.
     */
    public readonly endpointUrl: URL;

    /**
     * The service client used to communicate with the datasync service.
     */
    public readonly serviceClient: ServiceClient;

    /**
     * Creates an instance of a DatasyncClient
     * 
     * Example usage:
     * 
     * ```ts
     * import { DatasyncClient } from "ms-datasync-client";
     * 
     * const client = new DatasyncClient("<datasync service endpoint>");
     * ```
     * 
     * @param endpointUrl - the URL to the Datasync service.
     */
    constructor(endpointUrl: string | URL);

    /**
     * Creates an instance of a DatasyncClient
     * 
     * Example usage:
     * 
     * ```ts
     * import { DatasyncClient, DatasyncClientOptions } from "ms-datasync-client";
     * 
     * const options: DatasyncClientOptions = {
     *   userAgentOptions: {
     *     userAgentPrefix: "MyApp ";
     *   }
     * };
     * const client = new DatasyncClient("<datasync service endpoint>", options);
     * ```
     * 
     * @param endpointUrl - the URL to the Datasync service.
     * @param clientOptions - configuration used to send requests to the service.
     */
    constructor(endpointUrl: string | URL, clientOptions: DatasyncClientOptions);

    /**
     * Creates an instance of a DatasyncClient
     * 
     * Example usage:
     * 
     * ```ts
     * import { DefaultAzureCredential } from "@azure/identity";
     * import { DatasyncClient } from "ms-datasync-client";
     * 
     * const credential = new DefaultAzureCredential();
     * const client = new DatasyncClient("<datasync service endpoint>", credential);
     * ```
     * 
     * @param endpointUrl - the URL to the Datasync service.
     * @param credential - the credential to be used for authentication.
     */
    constructor(endpointUrl: string | URL, credential: TokenCredential);

    /**
     * Creates an instance of a DatasyncClient
     * 
     * Example usage:
     * 
     * ```ts
     * import { DefaultAzureCredential } from "@azure/identity";
     * import { DatasyncClient, DatasyncClientOptions } from "ms-datasync-client";
     * 
     * const credential = new DefaultAzureCredential();
     * const options: DatasyncClientOptions = {
     *   userAgentOptions: {
     *     userAgentPrefix: "MyApp ";
     *   }
     * };
     * const client = new DatasyncClient("<datasync service endpoint>", credential, options);
     * ```
     * 
     * @param endpointUrl - the URL to the Datasync service.
     * @param credential - the credential to be used for authentication.
     * @param clientOptions - configuration used to send requests to the service.
     */
    constructor(endpointUrl: string | URL, credential: TokenCredential, clientOptions: DatasyncClientOptions);

    /**
     * Creates an instance of a DatasyncClient
     * 
     * Example usage:
     * 
     * ```ts
     * import { DatasyncClient } from "ms-datasync-client";
     * 
     * const client = new DatasyncClient("<datasync service endpoint>");
     * ```
     * 
     * @param endpointUrl - the URL to the Datasync service.
     * @param credentialOrOptions - either the credential to be used, or the optional configuration used to send requests to the service.
     * @param clientOptions - optional configuration used to send requests to the service.
     */
    constructor(endpointUrl: string | URL, credentialOrOptions?: TokenCredential | DatasyncClientOptions, clientOptions?: DatasyncClientOptions) {
        const baseUrl = (typeof endpointUrl === "string") ? new URL(endpointUrl) : endpointUrl;
        const credential = (typeof credentialOrOptions !== "undefined" && isTokenCredential(credentialOrOptions)) ? credentialOrOptions : undefined;
        const options = ((typeof credentialOrOptions !== "undefined" && !isTokenCredential(credentialOrOptions)) ? credentialOrOptions : clientOptions) || {};

        if (!validate.isDatasyncServiceUrl(baseUrl)) {
            throw new InvalidArgumentError(`"${baseUrl.href}" is not valid as a datasync service`, "endpointUrl");
        }

        const packageDetails = `Datasync/${pkg.version}`;
        const userAgentPrefix = options.userAgentOptions?.userAgentPrefix ? `${options.userAgentOptions.userAgentPrefix} ${packageDetails}` : packageDetails;

        const policies = options.additionalPolicies || [];
        const clientPolicy = datasyncClientPolicy(options);
        policies.push({ policy: clientPolicy, position: "perRetry" });
        if (isTokenCredential(credential)) {
            const authPolicy = bearerTokenAuthenticationPolicy({ credential, scopes: options.scopes || `${new URL("/.default", baseUrl).href}` });
            policies.push({ policy: authPolicy, position: "perRetry"});
        }

        this.serviceClient = new ServiceClient({
            // Options that can be overridden
            timeout: 60000,

            ...options,

            // Options that cannot be overridden
            additionalPolicies: policies,
            allowInsecureConnection: baseUrl.protocol === "http:",
            endpoint: baseUrl.href,
            requestContentType: "application/json; charset=utf-8",
            userAgentOptions: {
                userAgentPrefix
            }
        });
      
        this.clientOptions = options;
        this.credential = credential;
        this.endpointUrl = baseUrl;
    }

    /**
     * Returns a table reference for a remote table.
     * 
     * @param tableName - the name of the table.
     * @param tablePath - the endpoint for the table (will be computed if not provided)
     * @returns the table reference.
     */
    getRemoteTable<T extends DataTransferObject>(tableName: string, tablePath?: string): DatasyncTable<T> {
        if (!validate.isTableName(tableName)) {
            throw new InvalidArgumentError("Invalid table name", "tableName");
        }
        let path: string | undefined;
        if (typeof tablePath !== "undefined") {
            path = tablePath;
        } else if (typeof this.clientOptions.tablePathResolver !== "undefined") {
            path = this.clientOptions.tablePathResolver(tableName);
        } else {
            path = `/tables/${tableName.toLowerCase()}`;
        }
        if (!validate.isNotEmpty(path)) {
            throw new InvalidArgumentError("Cannot create path for table", "tableName");
        }
        const endpoint = new URL(path, this.endpointUrl);
        return new RemoteTable<T>(this.serviceClient, tableName, endpoint, this.clientOptions);
    }
}
