import {
    MobileDataClient
} from "./mobileDataClient"

/**
 * Represents a table in the Azure Mobile Apps backend.
 */
export class MobileServiceTable {
    private readonly _client: MobileDataClient;
    private readonly _tableName: string;

    /**
     * Represents a table in the Azure Mobile Apps backend.
     * @param connectionString Name of the table.
     * @param client Client for the table.
     */
    constructor(tableName: string, client: MobileDataClient) {
        this._tableName = tableName;
        this._client = client;
    }

    get tableName() : string {
        return this._tableName;
    }
}
