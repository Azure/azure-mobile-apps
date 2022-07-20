// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import * as http from '../../../src/http/client';
import { runBasicHttpTests } from './basicHttpTests';

describe('src/http/client/fetchClientHandler', function () {
    this.slow(1500);        // Web calls take longer

    const getClient = () => new http.FetchClientHandler();

    describe('#FetchClientHandler', () => {
        runBasicHttpTests(getClient);

        it('can use default headers', async () => {
            const clientConfig: http.FetchClientOptions = {
                headers: {
                    'X-Zumo-Request': 'abcd'
                }
            };
            const client = new http.FetchClientHandler(clientConfig);

            const request = new http.HttpRequestMessage(
                http.HttpMethod.Get, 
                new URL("https://httpbin.org/anything")
            );
            const response = await client.sendAsync(request);
    
            expect(response).to.be.instanceof(http.HttpResponseMessage);

            // content
            expect(response.content).to.be.a('string').and.to.have.length.greaterThan(0);
            if (typeof response.content === 'string') {
                const obj = JSON.parse(response.content);
                expect(obj.headers).to.haveOwnProperty('X-Zumo-Request');
                expect(obj.headers['X-Zumo-Request']).to.equal('abcd');
            }
        });
    });
})