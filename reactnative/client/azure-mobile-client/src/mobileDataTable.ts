import { MobileDataClient } from "./mobileDataClient";
import { MobileDataClientOptions } from "./mobileDataClientOptions";
import { TokenCredential } from '@azure/identity';
import { EntityTableData } from './models';

/**
 * Represents a table in the Azure Mobile Apps backend.
 */
abstract class MobileDataTable<T extends EntityTableData> {
    private readonly _client: MobileDataClient;
    private readonly _clientOptions: MobileDataClientOptions;
    private readonly _credential: TokenCredential;
    private readonly _url: string;

    /**
     * Represents a table in the Azure Mobile Apps backend.
     * @param connectionString Name of the table.
     * @param client Client for the table.
     */
    constructor(client: MobileDataClient, tableName: string) {
        this._url = `tables/` + tableName;
        this._client = client;
        this._credential = client.credentials;
        this._clientOptions = client.options;
    }

    get url(): string {
        return this._url;
    }

    abstract create<T extends EntityTableData>(): T;

    async getItem<T extends EntityTableData>(id: string): Promise<T> {
        const local: T = this.create();
        local.deleted = true;
        local.id = id;
        local.updatedAt = new Date();
        local.version = "foo";

        return local;
    }
}
