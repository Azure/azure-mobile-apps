// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { ServiceClient } from "@azure/core-client";
import { PagedAsyncIterableIterator } from "@azure/core-paging";
import * as msrest from "@azure/core-rest-pipeline";
import { v4 as uuid } from "uuid";

import { DatasyncClientOptions } from "../DatasyncClient";
import { DatasyncTable } from "./DatasyncTable";
import { DataTransferObject, JsonReviver, Page, TableOperationOptions, TableQuery } from "./models";
import { ConflictError, InvalidArgumentError, validate } from "../utils";

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
 * Reviver for the DataTransferObject.
 * 
 * @param propertyName The name of the property
 * @param propertyValue The value of the property
 * @returns the value of the property
 */
const reviveDTO: JsonReviver = (propertyName: string, propertyValue: unknown) => {
    if (propertyName === "updatedAt" && typeof propertyValue === "string") {
        return new Date(propertyValue);
    }
    return propertyValue;
};

/**
 * The list of fields in the DTO.
 */
const dtoFields: Array<string> = [ "id", "updatedAt", "version", "deleted" ];

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
     * The reviver for this table.
     */
    public reviver?: JsonReviver;

    /**
     * The service client that will be used for any requests to the datasync service.
     */
    public readonly serviceClient: ServiceClient;

    /**
     * The relative path to the table endpoint.
     */
    public readonly tableEndpoint: string;

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
        this.tableEndpoint = endpoint.href.replace(/\/$/, ""); // Strip end slashes
        if (typeof clientOptions.tableReviver !== "undefined") {
            this.reviver = clientOptions.tableReviver(tableName);
        }
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
        const request = this.createRequest("POST", new URL(this.tableEndpoint), this.serialize(item), options);
        if (!isForcedRequest(options)) {
            request.headers.set("If-None-Match", "*");
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response);
        return this.deserialize<T>(request, response);
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
        const request = this.createRequest("DELETE", new URL(`${this.tableEndpoint}/${itemId}`), undefined, options);
        if (!isForcedRequest(options) && typeof item !== "string" && validate.isNotEmpty(item.version)) {
            request.headers.set("If-Match", `"${item.version}"`);
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response);
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
        const request = this.createRequest("GET", new URL(`${this.tableEndpoint}/${itemId}`), undefined, options);
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response);
        return this.deserialize<T>(request, response);
     }

    /**
     * Gets a page of items specified by the provided filter.
     * 
     * @param query - the filter used to restrict the items being retrieved.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to a page of stored items when complete.
     */
    public async getPageOfItems(query?: TableQuery, options?: TableOperationOptions): Promise<Page<T>> {
        const requestUrl = new URL(this.tableEndpoint);
        requestUrl.search = this.getSearchParams(query);
        const request = this.createRequest("GET", requestUrl, undefined, options);
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response);
        return this.deserialize<Page<Partial<T>>>(request, response);
    }

    /**
     * Retrieves an async list of items specified by the provided filter.
     * 
     * @param query - the filter used to restrict the items being retrieved.
     * @param options - the options to use on this request.
     * @returns An async iterator over the results.
     */
    public listItems(query?: TableQuery, options?: TableOperationOptions): PagedAsyncIterableIterator<Partial<T>, Page<T>> {
        const iterator = this.listItemsIterator(query, options);

        return {
            next: () => iterator.next(),
            [Symbol.asyncIterator]() { return this; },
            byPage: () => this.listItemsByPage(query, options)
        };
    }

    /**
     * Async iterator for each item in the list of results.
     * 
     * @param query - the query to execute.
     * @param options - the optios for each request.
     * @returns An async iterable over the items in the result set.
     */
    private async *listItemsIterator(query?: TableQuery, options?: TableOperationOptions): AsyncIterableIterator<Partial<T>> {
        for await (const page of this.listItemsByPage(query, options)) {
            for (const item of page.items) {
                yield item;
            }
        }
    }

    /**
     * Async iterator for each page of items.
     * 
     * @param query - the query to execute.
     * @param options - the options for each request.
     * @returns An async iterable over the pages of items.
     */
    private async *listItemsByPage(query?: TableQuery, options?: TableOperationOptions): AsyncIterableIterator<Page<T>> {
        const requestUrl = new URL(this.tableEndpoint);
        requestUrl.search = this.getSearchParams(query);

        let nextLink: URL | undefined = requestUrl;
        while (typeof nextLink !== "undefined") {
            const request = this.createRequest("GET", nextLink, undefined, options);
            const response = await this.serviceClient.sendRequest(request);
            this.throwIfNotSuccessful(request, response);
            const result = this.deserialize<Page<Partial<T>>>(request, response);

            yield { ...result };

            nextLink = typeof result.nextLink !== "undefined" ? new URL(result.nextLink) : undefined;
        }
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
        const request = this.createRequest("PUT", new URL(`${this.tableEndpoint}/${item.id}`), this.serialize(item), options);
        if (!isForcedRequest(options) && validate.isNotEmpty(item.version)) {
            request.headers.set("If-Match", `"${item.version}"`);
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response);
        return this.deserialize<T>(request, response);
    }

    /**
     * Updates an item in the remote store.  The ID of the item must be specified
     * in the partial.
     * 
     * @param item - the partial item with the ID.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the updated item.
     */
    public async updateItem(item: Partial<T>, options?: TableOperationOptions): Promise<T> {
        if (!validate.isEntityId(item.id)) {
            throw new InvalidArgumentError("Entity ID is not valid", "item");
        }
        const request = this.createRequest("PATCH", new URL(`${this.tableEndpoint}/${item.id}`), this.serialize(item), options);
        if (!isForcedRequest(options) && validate.isNotEmpty(item.version)) {
            request.headers.set("If-Match", `"${item.version}"`);
        }
        const response = await this.serviceClient.sendRequest(request);
        this.throwIfNotSuccessful(request, response);
        return this.deserialize<T>(request, response);
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
            allowInsecureConnection: this.clientOptions.allowInsecureConnection ?? url.href === "http:",
            body: content,
            headers: msrest.createHttpHeaders(),
            method: method,
            requestId: uuid(),
            timeout: options?.timeout || this.clientOptions.timeout || 60000,
            url: url.href,
            withCredentials: true
        };
        return request;
    }


    /**
     * Deserialize some JSON content into the appropriate type
     * 
     * @param request - the request that generated the content
     * @param response - the response holding the content
     * @returns the deserialized content
     * @throws RestError if the deserialized content is empty
     */
    private deserialize<U>(request: msrest.PipelineRequest, response: msrest.PipelineResponse) {
        if (typeof response.bodyAsText !== "string" || response.bodyAsText.length <= 0) {
            throw new msrest.RestError("No content was received", { code: "NO_CONTENT", statusCode: response.status, request, response });
        } else {
            return this.deserializeContent<U>(response.bodyAsText);
        }
    }

    /**
     * Deserializes some JSON content into the appropriate type.
     * 
     * @param content - the content to be deserialized.
     * @returns the deserialized object.
     */
    private deserializeContent<U>(content: string): U {
        return JSON.parse(content, (propName: string, propValue: unknown) => {
            if (dtoFields.indexOf(propName) > -1) {
                return reviveDTO(propName, propValue);
            }
            if (typeof this.reviver !== "undefined") {
                return this.reviver(propName, propValue);
            }
            return propValue;
        });
    }

    /**
     * Converts a TableQuery into the appropriate query string for sending to the service.
     * 
     * @param query - the query to be executed.
     */
    private getSearchParams(query?: TableQuery): string {
        const params: Array<string> = [];
        if (typeof query !== "undefined") {
            if (typeof query.filter !== "undefined" && query.filter.length > 0) {
                params.push(`$filter=${encodeURI(query.filter)}`);
            }
            if (typeof query.orderBy !== "undefined" && query.orderBy.length > 0) {
                params.push(`$orderby=${encodeURI(query.orderBy.join(","))}`);
            }
            if (typeof query.selection !== "undefined" && query.selection.length > 0) {
                params.push(`$select=${encodeURI(query.selection.join(","))}`);
            }
            if (typeof query.skip !== "undefined" && query.skip > 0) {
                params.push(`$skip=${query.skip}`);
            }
            if (typeof query.top !== "undefined" && query.top > 0) {
                params.push(`$top=${query.top}`);
            }
            if (typeof query.includeCount !== "undefined" && query.includeCount) {
                params.push("$count=true");
            }
            if (typeof query.includeDeletedItems !== "undefined" && query.includeDeletedItems) {
                params.push("__includedeleted=true");
            }
        }
        return params.join("&");
    }

    /**
     * Serializes the object provided to JSON.
     * 
     * @param content - the content to be serialized.
     * @returns the serialized string.
     */
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    private serialize(content: any): string {
        return JSON.stringify(content);
    }

    /**
     * If the response from the server is not successful, then throw a RestError.
     * 
     * @param request - the PipelineRequest object.
     * @param response - the PipelineResponse object.
     * @param ensureResponseContent - if true and the request was successful, a body must be provided.
     */
    private throwIfNotSuccessful(request: msrest.PipelineRequest, response: msrest.PipelineResponse) {
        if (response.status == 409 || response.status == 412) {
            throw new ConflictError(request, response, this.deserialize<T>(request, response));
        }
        if (response.status < 200 || response.status > 299) {
            throw new msrest.RestError("Service request was not successful", { code: "HTTP_ERROR", statusCode: response.status, request, response });
        }
    }
}