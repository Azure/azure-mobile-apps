// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import * as http from '../../src/http/client';
import { ServiceResponse } from '../../src/http';

const url = new URL('https://ds.azurewebsites.net/tables/foo');

describe('src/http/serviceResponse', () => {
    describe('#HttpResponseMessage', () => {
        it('sets all the variables - minimum', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const response = new http.HttpResponseMessage(200, "OK", request);
            const sut = new ServiceResponse(response);

            expect(sut.content).to.be.empty;
            expect(sut.etag).to.be.undefined;
            expect(sut.hasContent).to.be.false;
            expect(sut.headers).to.be.empty;
            expect(sut.isConflictStatusCode).to.be.false;
            expect(sut.isSuccessStatusCode).to.be.true;
            expect(sut.reasonPhrase).to.equal('OK');
            expect(sut.statusCode).to.equal(200);
        });

        it('sets all the variables - full', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const response = new http.HttpResponseMessage(200, "OK", request, { 'X-ZUMO-TEST': '1234' }, "body-text");
            const sut = new ServiceResponse(response);

            expect(sut.content).to.equal('body-text');
            expect(sut.etag).to.be.undefined;
            expect(sut.hasContent).to.be.true;
            expect(sut.headers).to.eql({ 'x-zumo-test': '1234' });
            expect(sut.isConflictStatusCode).to.be.false;
            expect(sut.isSuccessStatusCode).to.be.true;
            expect(sut.reasonPhrase).to.equal('OK');
            expect(sut.statusCode).to.equal(200);
        });

        it('sets isSuccessStatusCode to true', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            for (let statusCode = 200; statusCode < 299; statusCode++) {
                const response = new http.HttpResponseMessage(statusCode, '', request, { 'X-ZUMO-TEST': '1234' }, "body-text");
                const sut = new ServiceResponse(response);
                expect(sut.isSuccessStatusCode).to.be.true;
            }
        });

        it('sets isSuccessStatusCode to false', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            for (let statusCode = 300; statusCode < 499; statusCode++) {
                const response = new http.HttpResponseMessage(statusCode, '', request, { 'X-ZUMO-TEST': '1234' }, "body-text");
                const sut = new ServiceResponse(response);
                expect(sut.isSuccessStatusCode).to.be.false;
            }
        });

        it('sets isConflictStatusCode', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
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
                const response = new http.HttpResponseMessage(key, '', request, { 'X-ZUMO-TEST': '1234' }, "body-text");
                const sut = new ServiceResponse(response);
                expect(sut.isConflictStatusCode).to.equal(value);
            });
        });

        it('SELECTION extracts the etag when set (strong, no quotes)', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const response = new http.HttpResponseMessage(201, '', request, { 'ETag': '1234' }, "body-text");
            const sut = new ServiceResponse(response);
            expect(sut.etag).to.equal('1234');
        });

        it('extracts the etag when set (strong, quotes)', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const response = new http.HttpResponseMessage(201, '', request, { 'ETag': '"1234"' }, "body-text");
            const sut = new ServiceResponse(response);
            expect(sut.etag).to.equal('1234');
        });

        it('extracts the etag when set (weak, quotes)', () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, url);
            const response = new http.HttpResponseMessage(201, '', request, { 'ETag': 'W/"1234"' }, "body-text");
            const sut = new ServiceResponse(response);
            expect(sut.etag).to.equal('W/"1234"');
        });
    });
});