import { PagedAsyncIterableIterator, PageSettings } from "@azure/core-paging";
import { DataTransferObject, DataTransferPage } from "./DataTransferObject";
import { IDataTable } from "./IDataTable";
import { TableQuery } from "./TableQuery";

export class RemoteTable<T extends DataTransferObject> implements IDataTable<T> {
    /**
     * Gets the name of the table.
     * 
     * @returns {string} The name of the table
     */
    get tableName(): string {
        throw new Error("Method not implemented.");
    }

    /**
     * Creates a new item in the table.  The item does not need
     * to have an ID.  If it does have an ID, it must be a globally
     * unique string.
     * 
     * @param item The item to be created.
     * @param signal An AbortSignal to monitor.
     * @returns A promise that resolves to the stored item.
     */
    createItemAsync(item: T, signal?: AbortSignal): Promise<T> {
        throw new Error("Method not implemented.");
    }

    /**
     * Deletes an existing item in the table.  The item must have
     * a globally unique ID. If the item has a version, the version
     * must match.
     * 
     * @param item The item to be deleted.
     * @param signal An AbortSignal to monitor.
     * @returns A promise that resolves when the operation is complete.
     */
    deleteItemAsync(item: T, signal?: AbortSignal): Promise<void> {
        throw new Error("Method not implemented.");
    }

    /**
     * Retrieves an existing item in the table.  The item must have
     * a globally unique ID.
     * 
     * @param id The ID of the item to be retrieved. 
     * @param signal An AbortSignal to monitor.
     * @returns A promise that resolves to the stored item.
     */
    getItemAsync(id: string, signal?: AbortSignal): Promise<T> {
        throw new Error("Method not implemented.");
    }

    /**
     * Retrieves a single page of items from the table according to the
     * query.  If no query is specified, the first page of items according
     * to the server will be retrieved.
     * 
     * @param query The query to use for retrieving the page of items.
     * @param signal An AbortSignal to monitor.
     * @returns A promise that resolves to a page of items.
     */
    getPageOfItemsAsync(query?: TableQuery, signal?: AbortSignal): Promise<DataTransferPage<T>> {
        throw new Error("Method not implemented.");
    }

    /**
     * Retrieves an async iterable of the items returned by the query.  If no query
     * is specified, all items are returned.
     * 
     * @param query The query to use for retrieving the items in the table.
     * @returns An async iterable object of items.
     */
    listItemsAsync(query?: TableQuery): PagedAsyncIterableIterator<T> {
        const iter = this.getPageOfItemsAsync()
        throw new Error("Method not implemented.");
    }

    /**
     * Replaces an item in the table with a new version of the item.
     * The item must have a globally unique ID.  If there is a version
     * in the current item, then the version on the backend must match.
     * 
     * @param item The new contents of the item. 
     * @param signal An AbortSignal to monitor.
     * @returns A promise that resolves to the stored item.
     */
    replaceItemAsync(item: T, signal?: AbortSignal): Promise<T> {
        throw new Error("Method not implemented.");
    }
    
}