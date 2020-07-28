import { TokenCredential } from "@azure/identity";
import { MobileDataClientOptions } from './mobileDataClientOptions';

export class MobileDataClient {
    private readonly _url: string;
    private readonly _credential: TokenCredential | undefined;
    private readonly _options: MobileDataClientOptions;

    constructor(url: string, credential?: TokenCredential, options?: MobileDataClientOptions) {
        this._url = url;
        this._credential = credential ? credential : undefined;
        this._options = options ? options : {};
    }

    get url(): string {
        return this._url;
    }
}
