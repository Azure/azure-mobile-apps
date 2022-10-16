// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { PagedAsyncIterableIterator } from "@azure/core-paging";
import { DataTransferObject, Page, TableOperationOptions, TableQuery } from "./models";

export interface DatasyncTable<T extends DataTransferObject> {
    /**
     * Creates a new item in the table.  The item must not exist.
     * 
     * @param item - the item to create.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the stored item.
     */
    createItem(item: T, options?: TableOperationOptions): Promise<T>;

    /**
     * Deletes an existing item in the table.  If the item is provided,
     * then the deletion happens only if versions match.
     * 
     * @param item - the item to delete (by ID or item).
     * @param options - the options to use on this request.
     * @returns A promise that resolves when the operation is complete.
     */
    deleteItem(item: T | string, options?: TableOperationOptions): Promise<void>;

    /**
     * Retrieves an existing item from the table.
     * 
     * @param itemId - the ID of the item to retrieve.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the stored item.
     */
    getItem(itemId: string, options?: TableOperationOptions): Promise<T>;

    /**
     * Gets a page of items specified by the provided filter.
     * 
     * @param query - the filter used to restrict the items being retrieved.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to a page of stored items when complete.
     */
    getPageOfItems(query?: TableQuery, options?: TableOperationOptions): Promise<Page<Partial<T>>>;

    /**
     * Retrieves an async list of items specified by the provided filter.
     * 
     * @param query - the filter used to restrict the items being retrieved.
     * @param options - the options to use on this request.
     * @returns An async iterator over the results.
     */
    listItems(query?: TableQuery, options?: TableOperationOptions): PagedAsyncIterableIterator<Partial<T>, Page<Partial<T>>>;

    /**
     * Replaces an item in the remote store.  If the item has an version, the
     * item is only replaced if the version matches the remote version.
     * 
     * @param item - the item with new data for the replaced data.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the stored item.
     */
    replaceItem(item: T, options?: TableOperationOptions): Promise<T>;

    /**
     * Updates an item in the remote store.  The ID of the item must be specified
     * in the partial.
     * 
     * @param item - the partial item with the ID.
     * @param options - the options to use on this request.
     * @returns A promise that resolves to the updated item.
     */
    updateItem(item: Partial<T>, options?: TableOperationOptions): Promise<T>;
}