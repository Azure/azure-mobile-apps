import { MobileDataClient } from "./mobileDataClient";
import { EntityTableData } from './models';
import { URLBuilder, ServiceClient } from '@azure/core-http';

/**
 * Represents a table in the Azure Mobile Apps backend.
 */
export abstract class DataTable<T extends EntityTableData> extends ServiceClient {
    private readonly _client: MobileDataClient;
    private readonly _url: string;

    /**
     * Represents a table in the Azure Mobile Apps backend.
     * @param connectionString Name of the table.
     * @param client Client for the table.
     */
    constructor(client: MobileDataClient, tableName: string) {
        super(client.credentials, client.options)

        this._client = client;

        const builder = URLBuilder.parse(this._client.url);
        builder.appendPath(`tables/${tableName.toLowerCase()}s`);

        this._url = builder.toString();
    }

    get url(): string {
        return this._url;
    }

    abstract create(bodyAsText: string): T;

    async getItem(id: string): Promise<T> {
        return this.create("foo: " + id);
    }

    async getItems(): Promise<T[]> {
        const items: T[]  = await Promise.all([this.getItem("1"), this.getItem("2"), this.getItem("3")])
            .then(values => {
                return values;
            })

        return items;
    }
}
