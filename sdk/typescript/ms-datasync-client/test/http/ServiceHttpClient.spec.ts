// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect, use } from 'chai';
import chaiString from 'chai-string';
import { MockHttpClient } from '../helpers/MockHttpClient';
import { ArgumentError } from '../../src/errors';
import { ServiceHttpClient, ServiceHttpClientOptions } from '../../src/http';

use(chaiString);

describe('http/ServiceHttpClient', () => {
    describe('#constructor', () => {
        it('builds a HTTP pipeline with a string endpoint', () => {
            const sut = new ServiceHttpClient('https://ds.azurewebsites.net');
            expect(sut.apiVersion).to.equal('3.0.0');
            expect(sut.endpoint.href).to.equal('https://ds.azurewebsites.net/');
        });

        it('builds a HTTP pipeline with a URL endpoint', () => {
            const sut = new ServiceHttpClient(new URL('https://ds.azurewebsites.net'));
            expect(sut.apiVersion).to.equal('3.0.0');
            expect(sut.endpoint.href).to.equal('https://ds.azurewebsites.net/');
        });

        it('throws on an invalid (string) endpoint', () => {
            expect(() => { new ServiceHttpClient('file:///?mode=rwc'); }).to.throw(ArgumentError);
        });

        it('throws on an invalid (URL) endpoint', () => {
            expect(() => { new ServiceHttpClient(new URL('http://ds.azurewebsites.net')); }).to.throw(ArgumentError);
        });

        it('sets the API version based on options', () => {
            const options: ServiceHttpClientOptions = {
                apiVersion: '2.0.0'
            };
            const sut = new ServiceHttpClient(new URL('https://ds.azurewebsites.net'), options);
            expect(sut.apiVersion).to.equal('2.0.0');
        });
    });

    describe('#sendRequest', () => {
        it('can handle a basic request/response', async () => {
            const mock = new MockHttpClient();
            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });

            mock.addResponse(200, '{"error":"foo"}', { 'Content-Type': 'application/json' });
            const request = mock.createRequest('GET', '/foo');
            const response = await client.sendRequest(request);

            expect(response).to.not.be.undefined;
            expect(response.status).to.equal(200);
            expect(response.bodyAsText).to.equal('{"error":"foo"}');
        });
    });
});
