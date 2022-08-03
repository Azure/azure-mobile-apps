// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect, use } from 'chai';
import chaiString from 'chai-string';
import { MockHttpClient } from '../helpers/MockHttpClient';
import { ArgumentError } from '../../src/errors';
import { HttpMethod, ServiceHttpClient, ServiceHttpClientOptions } from '../../src/http';
import { ServiceRequest } from '../../src/http/ServiceRequest';

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

        it('adds the required headers automatically', async () => {
            const mock = new MockHttpClient();
            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });

            mock.addResponse(200, '{"error":"foo"}', { 'Content-Type': 'application/json' });
            const request = mock.createRequest('GET', '/foo');
            await client.sendRequest(request);

            expect(request.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(request.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
        });

        it('can set a userAgentPrefix', async () => {
            const mock = new MockHttpClient();
            const client = new ServiceHttpClient('http://localhost', { 
                httpClient: mock,
                userAgentOptions: {
                    userAgentPrefix: 'test-wumpus'
                }
            });

            mock.addResponse(200, '{"error":"foo"}', { 'Content-Type': 'application/json' });
            const request = mock.createRequest('GET', '/foo');
            await client.sendRequest(request);

            expect(request.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(request.headers.get('x-zumo-version')).to.startWith('test-wumpus Datasync/5.0.0');
        });

        it('can override the apiVersion', async () => {
            const mock = new MockHttpClient();
            const client = new ServiceHttpClient('http://localhost', { 
                httpClient: mock,
                apiVersion: '2.1.0'
            });

            mock.addResponse(200, '{"error":"foo"}', { 'Content-Type': 'application/json' });
            const request = mock.createRequest('GET', '/foo');
            await client.sendRequest(request);

            expect(request.headers.get('zumo-api-version')).to.equal('2.1.0');
        });
    });

    describe('#sendServiceRequest', () => {
        it('can send a DELETE message and get a 204 response back', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(204);

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.DELETE, '/tables/foo/id');
            const response = await client.sendServiceRequest(request);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.method).to.equal('DELETE');
            expect(req.url).to.equal('http://localhost/tables/foo/id');

            // Check that serviceResponse is constructed properly
            expect(response.content).to.be.undefined;
            expect(response.etag).to.be.undefined;
            expect(response.hasContent).to.be.false;
            expect(response.hasValue).to.be.false;
            expect(response.isConflictStatusCode).to.be.false;
            expect(response.isSuccessStatusCode).to.be.true;
            expect(response.statusCode).to.equal(204);
            expect(response.value).to.be.undefined;
        });
    });
});
