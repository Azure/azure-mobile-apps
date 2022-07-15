// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import chai, { assert, expect } from 'chai';
import chaiAsPromised from 'chai-as-promised';
import * as http from '../../../src/http/client';

chai.use(chaiAsPromised);

const url = new URL('https://jsonplaceholder.typicode.com/posts/1');

/* eslint-disable @typescript-eslint/no-unused-vars */
class TestDelegatingHandler extends http.DelegatingHandler {
    count = 0;

    async sendAsync(request: http.HttpRequestMessage, signal?: AbortSignal): Promise<http.HttpResponseMessage> {
        this.count++;
        return super.sendAsync(request, signal);
    }
}

class TestClientHandler extends http.HttpClientHandler {
    count = 0;

    async sendAsync(request: http.HttpRequestMessage, signal?: AbortSignal): Promise<http.HttpResponseMessage> {
        this.count++;
        assert.isUndefined(signal); // avoids the TS6133 error since we don't use it during tests.
        return new http.HttpResponseMessage(200, "OK", request);
    }
}
/* eslint-enable @typescript-eslint/no-unused-vars */

describe('src/http/client/clientHandlers', () => {
    describe('#DelegatingHandler', () => {
        it('has the correct handlerType', () => {
            const sut = new TestDelegatingHandler();

            assert.equal(sut.handlerType, "DelegatingHandler");
        });

        it('throws when there is no inner handler', () => {
            const sut = new TestDelegatingHandler();
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);

            expect(sut.sendAsync(request)).to.be.rejectedWith(http.HttpError);
        });

        it('works calls inner handler', async () => {
            const clientHandler = new TestClientHandler();
            const sut = new TestDelegatingHandler(clientHandler);
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);

            const response = await sut.sendAsync(request);

            assert.equal(response.statusCode, 200);
            assert.equal(clientHandler.count, 1);
        });
    });

    describe('#HttpClientHandler', () => {
        it('has the correct handlerType', () => {
            const sut = new TestClientHandler();

            assert.equal(sut.handlerType, "HttpClientHandler");
        });
    });
});