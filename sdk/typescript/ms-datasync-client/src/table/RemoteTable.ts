// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/* eslint-disable @typescript-eslint/no-unused-vars */

import { ServiceClient } from "@azure/core-client";
import { PagedAsyncIterableIterator } from "@azure/core-paging";
import * as msrest from "@azure/core-rest-pipeline";
import { v4 as uuid } from "uuid";

import { DatasyncClientOptions } from "../DatasyncClient";
import { DatasyncTable } from "./datasyncTable";
import { DataTransferObject, Page, TableOperationOptions, TableQuery } from "./models";
import { InvalidArgumentError } from "../utils/errors";
import * as validate from "../utils/validate";

/**
 * Helper method to determine if the operation should be forced or not.
 * 
 * @param options - the options for the request.
 * @returns true if the operation should be forced; false otherwise.
 */
function isForcedRequest(options?: TableOperationOptions): boolean {
    return options?.force ?? false;
}

/**
 * An implementation of the DatasyncTable interface that sends requests to
 * a remote datasync service.
 */
export class RemoteTable<T extends DataTransferObject> implements DatasyncTable<T> {
    /**
     * The default client options to use in formulating HTTP requests.
     */
    public readonly clientOptions: DatasyncClientOptions;

    /**
     * The service client that will be used for any requests to the datasync service.
     */
    public readonly serviceClient: ServiceClient;

    /**
     * The relative path to the table endpoint.
     */
    public readonly tableEndpoint: URL;

    /**
     * The name of the table.
     */
    public readonly tableName: string;

    /**
     * Creates a new RemoteTable connection.  Do not use this method.  Instead, use
     * getRemoteTable() on the DatasyncClient instance.
     * 
     * @param serviceClient - the service client to use for communicating with the datasync service.
     * @param tableName - the name of the table.
     * @param endpoint - the absolute HTTP endpoint to the table.
     * @param clientOptions - the client options to use.
     */
    constructor(serviceClient: ServiceClient, tableName: string, endpoint: URL, clientOptions: DatasyncClientOptions) {
        this.clientOptions = clientOptions;
        this.serviceClient = serviceClient;
        this.tableName = tableName;
        this.tableEndpoint = endpoint;
    }

    /**
     * Creates a new item in the table.  The item must not exist.
     * 
     * @param item - the item to create.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the stored item.
     */
    public async createItem(item: T, options?: TableOperationOptions): Promise<T> {
        if (!validate.isEntityId(item.id)) {
            throw new InvalidArgumentError("Entity ID is not valid", "item");
        }
        const request = this.createRequest("POST", this.tableEndpoint, this.serialize(item), options);
        if (!isForcedRequest(options)) {
            request.headers.set("If-None-Match", "*");
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response, true);
        return this.deserialize<T>(response.bodyAsText);
    }

    /**
     * Deletes an existing item in the table.  If the item is provided,
     * then the deletion happens only if versions match.
     * 
     * @param item - the item to delete (by ID or item).
     * @param options - the options to use on this request.
     * @returns A promise that resolves when the operation is complete.
     */
     public async deleteItem(item: T | string, options?: TableOperationOptions): Promise<void> {
        const itemId: string = (typeof item === "string") ? item : item.id;
        if (!validate.isEntityId(itemId)) {
            throw new InvalidArgumentError("Entity ID is not valid", "item");
        }
        const request = this.createRequest("DELETE", new URL(itemId, this.tableEndpoint), undefined, options);
        if (!isForcedRequest(options) && typeof item !== "string" && validate.isNotEmpty(item.version)) {
            request.headers.set("If-Match", `"${item.version}"`);
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response, false);
    }

    /**
     * Retrieves an existing item from the table.
     * 
     * @param itemId - the ID of the item to retrieve.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the stored item.
     */
     public async getItem(itemId: string, options?: TableOperationOptions): Promise<T> {
        if (!validate.isEntityId(itemId)) {
            throw new InvalidArgumentError("Entity ID is not valid", "itemId");
        }
        const request = this.createRequest("GET", new URL(itemId, this.tableEndpoint), undefined, options);
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response, true);
        return this.deserialize<T>(response.bodyAsText);
     }

    /**
     * Gets a page of items specified by the provided filter.
     * 
     * @param query - the filter used to restrict the items being retrieved.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to a page of stored items when complete.
     */
    public async getPageOfItems(query?: TableQuery, options?: TableOperationOptions): Promise<Page<Partial<T>>> {
        throw "not implemented";
    }

    /**
     * Retrieves an async list of items specified by the provided filter.
     * 
     * @param query - the filter used to restrict the items being retrieved.
     * @param options - the options to use on this request.
     * @returns An async iterator over the results.
     */
    listItems(query?: TableQuery, options?: TableOperationOptions): PagedAsyncIterableIterator<Partial<T>> {
        throw "not implemented";
    }

    /**
     * Replaces an item in the remote store.  If the item has an version, the
     * item is only replaced if the version matches the remote version.
     * 
     * @param item - the item with new data for the replaced data.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the stored item.
     */
    public async replaceItem(item: T, options?: TableOperationOptions): Promise<T> {
        if (!validate.isEntityId(item.id)) {
            throw new InvalidArgumentError("Entity ID is not valid", "item");
        }
        const request = this.createRequest("POST", this.tableEndpoint, this.serialize(item), options);
        if (!isForcedRequest(options) && validate.isNotEmpty(item.version)) {
            request.headers.set("If-Match", `"${item.version}"`);
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response, true);
        return this.deserialize<T>(response.bodyAsText);
    }

    /**
     * Internal method to construct a new request object.
     * 
     * @param method The HTTP method to execute.
     * @param url The URL of the request.
     * @param content If set, the JSON content for the request.
     * @param options If set, the operation options to set.
     * @returns The pipeline request object.
     */
    private createRequest(method: msrest.HttpMethods, url: URL, content?: string, options?: TableOperationOptions): msrest.PipelineRequest {
        const request: msrest.PipelineRequest = {
            abortSignal: options?.abortSignal,
            allowInsecureConnection: this.clientOptions?.allowInsecureConnection ?? url.href === "http:",
            body: content,
            headers: msrest.createHttpHeaders(),
            method: method,
            requestId: uuid(),
            timeout: options?.timeout || this.clientOptions?.timeout || 60000,
            url: url.href,
            withCredentials: true
        };
        return request;
    }

    /**
     * Deserializes some JSON content into the appropriate type.
     * 
     * @param content - the content to be deserialized.
     * @returns the deserialized object.
     * @throws SyntaxError if the JSON is invalid.
     */
    private deserialize<U>(content?: string | null): U {
        throw "not implemented";
    }

    /**
     * Serializes the object provided to JSON.
     * 
     * @param content - the content to be serialized.
     * @returns the serialized string.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private serialize(content: any): string {
        throw "not implemented";
    }

    /**
     * If the response from the server is not successful, then throw a RestError.
     * 
     * @param request - the PipelineRequest object.
     * @param response - the PipelineResponse object.
     * @param ensureResponseContent - if true and the request was successful, a body must be provided.
     */
    private throwIfNotSuccessful(request: msrest.PipelineRequest, response: msrest.PipelineResponse, ensureResponseContent: boolean) {
        if ((response.status == 409 || response.status == 412) && validate.isNotEmpty(response.bodyAsText)) {
            throw "not implemented";
        }
        if (response.status < 200 || response.status > 299) {
            throw new msrest.RestError("Service request was not successful", { code: "HTTP_ERROR", statusCode: response.status, request, response });
        }
        if (ensureResponseContent && !validate.isNotEmpty(response.bodyAsText)) {
            throw new msrest.RestError("No content was received", { code: "NO_CONTENT", statusCode: response.status, request, response });
        }
    }
}