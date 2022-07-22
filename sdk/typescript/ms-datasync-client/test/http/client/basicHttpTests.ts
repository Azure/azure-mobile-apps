// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { HttpClientHandler, HttpMethod, HttpRequestMessage, HttpResponseMessage } from '../../../src/http/client';
import { expect } from 'chai';

/**
 * A suite of tests to test basic HTTP functionality for a client.
 * 
 * @param getClient the function to return the client.
 */
export function runBasicHttpTests(getClient: () => HttpClientHandler) {
    it('GET-200-with-body', async () => {
        const client = getClient();

        const request = new HttpRequestMessage(
            HttpMethod.Get, 
            new URL("https://httpbin.org/anything")
        );
        const response = await client.sendAsync(request);

        expect(response).to.be.instanceof(HttpResponseMessage);

        // status code
        expect(response.statusCode, 'status code is 200').to.equal(200);
        expect(response.isConflictStatusCode, 'conflict status code is false').to.be.false;
        expect(response.isSuccessStatusCode, 'success status code is true').to.be.true;

        // headers
        expect(response.headers, 'response has a header').to.not.be.empty;

        // content
        expect(response.content, 'has content').to.be.a('string')
            .and.to.have.length.greaterThan(0);
        if (typeof response.content === 'string') {
            const obj = JSON.parse(response.content);
            expect(obj.method).to.be.equal("GET");
            expect(obj.data).to.be.equal("");
        }
    });

    it('GET-404-no-body', async () => {
        const client = getClient();

        const request = new HttpRequestMessage(
            HttpMethod.Get, 
            new URL("https://httpbin.org/status/404")
        );
        const response = await client.sendAsync(request);

        expect(response).to.be.instanceof(HttpResponseMessage);

        // status code
        expect(response.statusCode, 'status code is 404').to.equal(404);
        expect(response.isConflictStatusCode, 'conflict status code is false').to.be.false;
        expect(response.isSuccessStatusCode, 'success status code is false').to.be.false;

        // headers
        expect(response.headers, 'response has a header').to.not.be.empty;
        
        // content
        expect(response.content || '', 'has no content').to.be.equal('');
    });

    it('POST-200-with-body', async () => {
        const client = getClient();

        const request = new HttpRequestMessage(
            HttpMethod.Post,
            new URL("https://httpbin.org/anything"),
            "{\"abc\":\"1234\"}"
        );
        const response = await client.sendAsync(request);

        expect(response).to.be.instanceof(HttpResponseMessage);

        // status code
        expect(response.statusCode, 'status code is 200').to.equal(200);
        expect(response.isConflictStatusCode, 'conflict status code is false').to.be.false;
        expect(response.isSuccessStatusCode, 'success status code is true').to.be.true;

        // headers
        expect(response.headers, 'response has a header').to.not.be.empty;
        
        // content
        expect(response.content, 'has content').to.be.a('string')
            .and.to.have.length.greaterThan(0);
        if (typeof response.content === 'string') {
            const obj = JSON.parse(response.content);
            expect(obj.method).to.be.equal("POST");
            expect(obj.data).to.be.equal("{\"abc\":\"1234\"}");
        }
    });

    it('DELETE-204-no-body', async () => {
        const client = getClient();

        const request = new HttpRequestMessage(
            HttpMethod.Delete,
            new URL("https://httpbin.org/status/204")
        );
        const response = await client.sendAsync(request);

        expect(response).to.be.instanceof(HttpResponseMessage);

        // status code
        expect(response.statusCode, 'status code should be 204').to.equal(204);
        expect(response.isConflictStatusCode, 'conflict status code should be false').to.be.false;
        expect(response.isSuccessStatusCode, 'success status code should be true').to.be.true;

        // headers
        expect(response.headers, 'response has a header').to.not.be.empty;
        
        // content
        expect(response.content || '', 'has no content').to.be.equal('');
    });
}
