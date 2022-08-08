// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { AbortSignal } from '@azure/abort-controller';
import { PagedAsyncIterableIterator } from '@azure/core-paging';
import { HttpMethod, ServiceHttpClient, ServiceRequest, validate } from '../http';
import { DatasyncTable } from './DatasyncTable';
import { DataTransferObject, Page, TableQuery } from './models';

/**
 * Implementation of a remote connection to a table.
 */
export class RemoteTable<T extends DataTransferObject> implements DatasyncTable<T> {
    private tablePath: string;
    private client: ServiceHttpClient;

    /**
     * Creates a new table connection.
     * 
     * @param tablePath the path to the table (relative to the base URI of the service client)
     * @param client The service client.
     */
    constructor(tablePath: string, client: ServiceHttpClient) {
        validate.isRelativePath(tablePath, 'tablePath');
        
        this.tablePath = tablePath.replace(/\/*$/, ''); // Strip off any final separators
        this.client = client;
    }

    /**
     * Creates an item in the table.  The item must not exist.
     * 
     * @param item The item to create.
     * @param abortSignal An abort signal.
     * @returns A promise that resolves to the stored item.
     */
     public async createItem(item: T, abortSignal?: AbortSignal): Promise<T> {
        if (typeof item.id === 'string') {
            validate.isValidEntityId(item.id, 'item.id');
        }

        const request = new ServiceRequest(HttpMethod.POST, this.tablePath)
            .withContent(item)
            .withHeader('If-None-Match', '*')
            .requireResponseContent();
        const response = await this.client.sendServiceRequest(request, abortSignal);
        if (!response.isSuccessStatusCode) {
            throw this.standardError(request, response);
        }
        return response.value as T;
     }

     /**
      * Deletes an item in the table.  If the item is the generic type, then
      * the version of the item must match.
      * 
      * @param item The item to delete, or the item ID to delete.
      * @param abortSignal An abort signal.
      * @returns A promise that resolves when the item is deleted.
      */
     public async deleteItem(item: T | string, abortSignal?: AbortSignal): Promise<void> {
        const itemId = typeof item === 'string' ? item : item.id;
        validate.isValidEntityId(itemId, 'item');

        let request = new ServiceRequest(HttpMethod.DELETE, `${this.tablePath}/${itemId}`);
        if (typeof item !== 'string') {
            request = request.withVersionHeader(item.version);
        }
        const response = await this.client.sendServiceRequest(request, abortSignal);
        if (!response.isSuccessStatusCode) {
            throw this.standardError(request, response);
        }
     }
 
     /**
      * Retrieves an item in the table.
      * 
      * @param itemId The ID of the item to retrieve.
      * @param abortSignal An abort signal.
      * @returns A promise that resolves to the stored item.
      */
     public async getItem(itemId: string, abortSignal?: AbortSignal): Promise<T> {
        validate.isValidEntityId(itemId, 'itemId');

        const request = new ServiceRequest(HttpMethod.GET, `${this.tablePath}/${itemId}`)
            .requireResponseContent();
        const response = await this.client.sendServiceRequest(request, abortSignal);
        if (!response.isSuccessStatusCode) {
            throw this.standardError(request, response);
        }
        return response.value as T;
     }
 
     /**
      * Gets a single page of items from the server, according to the
      * filter.
      * 
      * @param filter the filter used to restrict the items being retrieved.
      * @param abortSignal An abort signal.
      * @returns A promise that resolves to a page of stored items.
      */
     public async getPageOfItems(filter?: TableQuery, abortSignal?: AbortSignal): Promise<Page<Partial<T>>> {
        const request = new ServiceRequest(HttpMethod.GET, this.tablePath)
            .withQueryString(this.createQueryString(filter))
            .requireResponseContent();
        const response = await this.client.sendServiceRequest(request, abortSignal);
        if (!response.isSuccessStatusCode) {
            throw this.standardError(request, response);
        }
        return response.value as Page<Partial<T>>;
     }
 
     /**
      * Retrieves a list of items specified by the filter.
      * 
      * @param filter the filter used to restrict the items to be retrieved.
      * @returns An async paged iterator over the results.
      */
     public listItems(filter?: TableQuery): PagedAsyncIterableIterator<Partial<T>> {
        throw new Error('Not implemented');
     }
 
     /**
      * Replaces the items with new data.
      * 
      * @param item the new data for the item.
      * @param abortSignal An abort signal.
      * @reutrns A promise that resolves to the stored item.
      */
     public async replaceItem(item: T, abortSignal?: AbortSignal): Promise<T> {
        validate.isValidEntityId(item.id, 'item.id');

        const request = new ServiceRequest(HttpMethod.PUT, `${this.tablePath}/${item.id}`)
            .withContent(item)
            .withVersionHeader(item.version)
            .requireResponseContent();
        const response = await this.client.sendServiceRequest(request, abortSignal);
        if (!response.isSuccessStatusCode) {
            throw this.standardError(request, response);
        }
        return response.value as T;
     }
}