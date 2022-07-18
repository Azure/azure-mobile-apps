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
        expect(response.statusCode).to.equal(200);
        expect(response.isConflictStatusCode).to.be.false;
        expect(response.isSuccessStatusCode).to.be.true;

        // headers
        expect(response.headers.has('server')).to.be.true;

        // content
        expect(response.content).to.be.a('string')
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
        expect(response.statusCode).to.equal(404);
        expect(response.isConflictStatusCode).to.be.false;
        expect(response.isSuccessStatusCode).to.be.false;

        // headers
        expect(response.headers.has('server')).to.be.true;
        
        // content
        expect(response.content || '').to.be.equal('');
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
        expect(response.statusCode).to.equal(200);
        expect(response.isConflictStatusCode).to.be.false;
        expect(response.isSuccessStatusCode).to.be.true;

        // headers
        expect(response.headers.has('server')).to.be.true;
        
        // content
        expect(response.content).to.be.a('string')
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
        expect(response.statusCode).to.equal(204);
        expect(response.isConflictStatusCode).to.be.false;
        expect(response.isSuccessStatusCode).to.be.true;

        // headers
        expect(response.headers.has('server')).to.be.true;
        
        // content
        expect(response.content || '').to.be.equal('');
    });
}