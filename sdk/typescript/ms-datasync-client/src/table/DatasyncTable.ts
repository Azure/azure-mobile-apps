// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { AbortSignal } from '@azure/abort-controller';
import { PagedAsyncIterableIterator } from '@azure/core-paging';
import { DataTransferObject, Page, TableQuery } from './models';

/**
 * Definition of the table specification, which provides
 * CRUDL on a remote or offline table.
 */
export interface DatasyncTable<T extends DataTransferObject> {
    /**
     * Creates an item in the table.  The item must not exist.
     * 
     * @param item The item to create.
     * @param abortSignal An abort signal.
     * @returns A promise that resolves to the stored item.
     */
    createItem(item: T, abortSignal?: AbortSignal): Promise<T>;

    /**
     * Deletes an item in the table.  If the item is the generic type, then
     * the version of the item must match.
     * 
     * @param item The item to delete, or the item ID to delete.
     * @param abortSignal An abort signal.
     * @returns A promise that resolves when the item is deleted.
     */
    deleteItem(item: T | string, abortSignal?: AbortSignal): Promise<void>;

    /**
     * Retrieves an item in the table.
     * 
     * @param itemId The ID of the item to retrieve.
     * @param abortSignal An abort signal.
     * @returns A promise that resolves to the stored item.
     */
    getItem(itemId: string, abortSignal?: AbortSignal): Promise<T>;

    /**
     * Gets a single page of items from the server, according to the
     * filter.
     * 
     * @param filter the filter used to restrict the items being retrieved.
     * @param abortSignal An abort signal.
     * @returns A promise that resolves to a page of stored items.
     */
    getPageOfItems(filter?: TableQuery, abortSignal?: AbortSignal): Promise<Page<Partial<T>>>;

    /**
     * Retrieves a list of items specified by the filter.
     * 
     * @param filter the filter used to restrict the items to be retrieved.
     * @returns An async paged iterator over the results.
     */
    listItems(filter?: TableQuery): PagedAsyncIterableIterator<Partial<T>>;

    /**
     * Replaces the items with new data.
     * 
     * @param item the new data for the item.
     * @param abortSignal An abort signal.
     * @reutrns A promise that resolves to the stored item.
     */
    replaceItem(item: T, abortSignal?: AbortSignal): Promise<T>;
}