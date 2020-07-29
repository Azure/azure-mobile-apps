import chai from "chai";
const should = chai.should();
import chaiAsPromised from "chai-as-promised";
chai.use(chaiAsPromised);
import chaiString from "chai-string";
chai.use(chaiString);

import { MobileDataClient } from "../src";
import { TokenCredential, GetTokenOptions, AccessToken } from '@azure/identity';

describe("Create MobileDataClient", function (): void {
    it("Sets properties", function (): void {
        const tokenCredential : TokenCredential = {
            getToken(_: string | string[], __?: GetTokenOptions): Promise<AccessToken | null> {
                return Promise.resolve(null);
            }
        };
        const url = "foo://url.com";
        const client = new MobileDataClient(url, tokenCredential, undefined);

        client.should.be.an.instanceof(MobileDataClient);
        should.equal(client.url, url);
        should.equal(client.credentials, tokenCredential);
    });
});
