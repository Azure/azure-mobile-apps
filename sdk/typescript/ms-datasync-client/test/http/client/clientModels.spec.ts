// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as http from '../../../src/http/client';

const url = new URL('https://jsonplaceholder.typicode.com/posts/1');

describe('src/http/client/clientModels', () => {
    describe('#HttpRequestMessage', () => {
        it('sets content-type when not specified', () => {
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234' };
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url, "content", headers);

            assert.isTrue(request.headers['Content-Type']?.startsWith('application/json'));
            assert.equal(request.headers['X-ZUMO-TEST'], '1234');
        });

        it('does not override content-type', () => {
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234', 'Content-Type': 'application/patch+json' };
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url, "content", headers); 

            assert.equal(request.headers['Content-Type'], 'application/patch+json');
            assert.equal(request.headers['X-ZUMO-TEST'], '1234');
        });

        it('sets content-type when setting content', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Post, url);
            request.content = "something";

            assert.isTrue(request.headers['Content-Type']?.startsWith('application/json'));
            assert.equal(request.content, "something");
        });

        it('does not override content-type when setting content', () => {
            const headers: http.HttpHeaders = { 'Content-Type': 'application/patch+json' };
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url, undefined, headers);
            request.content = "something";

            assert.equal(request.headers['Content-Type'], 'application/patch+json');
            assert.equal(request.content, "something");
        });

        it('clears content-type when clearing content', () => {
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234' };
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url, "content", headers);
            request.content = undefined;

            assert.isUndefined(request.content);
            assert.isUndefined(request.headers['Content-Type']);
        });

        it('can set headers', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url, "content"); // sets content-type
            request.headers = {};

            assert.isUndefined(request.headers['Content-Type']); // content-type no longer available.
        });

        it('can set method', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url, "content"); 
            assert.equal(request.method, http.HttpMethod.Get);
            
            request.method = http.HttpMethod.Delete;
            assert.equal(request.method, http.HttpMethod.Delete);

            request.method = http.HttpMethod.Patch;
            assert.equal(request.method, http.HttpMethod.Patch);

            request.method = http.HttpMethod.Post;
            assert.equal(request.method, http.HttpMethod.Post);

            request.method = http.HttpMethod.Put;
            assert.equal(request.method, http.HttpMethod.Put);
        });

        it('can set url', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            request.requestUri = new URL('http://localhost/foo');

            assert.equal(request.requestUri.href, 'http://localhost/foo');
        });
    });

    describe('#HttpResponseMessage', () => {
        it('sets all the variables - minimum', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const sut = new http.HttpResponseMessage(200, "OK", request);

            assert.isDefined(sut);
            assert.strictEqual(sut.statusCode, 200);
            assert.isTrue(sut.isSuccessStatusCode);
            assert.isFalse(sut.isConflictStatusCode);
            assert.strictEqual(sut.reasonPhrase, "OK");
            assert.equal(sut.requestMessage, request);
            assert.isUndefined(sut.content);
            assert.isEmpty(sut.headers);
        });

        it('sets all the variables - full', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234' };
            const sut = new http.HttpResponseMessage(200, "OK", request, headers, "body-text");

            assert.isDefined(sut);
            assert.strictEqual(sut.statusCode, 200);
            assert.isTrue(sut.isSuccessStatusCode);
            assert.isFalse(sut.isConflictStatusCode);
            assert.strictEqual(sut.reasonPhrase, "OK");
            assert.equal(sut.requestMessage, request);
            assert.equal(sut.content, "body-text");
            assert.isNotEmpty(sut.headers);
            assert.strictEqual(sut.headers['X-ZUMO-TEST'], '1234');
        });

        it('sets isSuccessStatusCode to true', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234' };

            for (let statusCode = 200; statusCode < 299; statusCode++) {
                const sut = new http.HttpResponseMessage(statusCode, "OK", request, headers, "body-text");
                assert.isTrue(sut.isSuccessStatusCode);
            }
        });

        it('sets isSuccessStatusCode to false', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234' };

            for (let statusCode = 400; statusCode < 599; statusCode++) {
                const sut = new http.HttpResponseMessage(statusCode, "Failed", request, headers, "body-text");
                assert.isFalse(sut.isSuccessStatusCode);
            }
        });

        it('sets isConflictStatusCode', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const headers: http.HttpHeaders = { 'X-ZUMO-TEST': '1234' };
            const codes: Map<number, boolean> = new Map<number, boolean>([
                [ 200, false ],
                [ 201, false ],
                [ 204, false ],
                [ 400, false ],
                [ 404, false ],
                [ 409, true ],
                [ 412, true ],
                [ 415, false ],
                [ 500, false ]
            ]);

            codes.forEach((value, key) => {
                const sut = new http.HttpResponseMessage(key, "Failed", request, headers, "body-text");
                assert.equal(sut.isConflictStatusCode, value);
            });
        });
    
        it('sets the reasonPhrase when not specified', () => {
            for (let statusCode = 100; statusCode < 599; statusCode++) {
                const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
                const response = new http.HttpResponseMessage(statusCode, '', request);
                assert.isDefined(response.reasonPhrase);
                assert.isNotEmpty(response.reasonPhrase);
            }
        });
    });
});