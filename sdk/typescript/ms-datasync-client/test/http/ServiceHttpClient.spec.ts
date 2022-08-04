// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert, expect, use } from 'chai';
import chaiString from 'chai-string';
import chaiAsPromised from 'chai-as-promised';
import { RequestBodyType } from '@azure/core-rest-pipeline';

import { MockHttpClient } from '../helpers/MockHttpClient';
import { ArgumentError, HttpError } from '../../src';
import { HttpMethod, ServiceHttpClient, ServiceHttpClientOptions, ServiceRequest, ServiceResponse } from '../../src/http';

use(chaiString);
use(chaiAsPromised);

function getBody(body?: RequestBodyType): string | undefined {
    if (typeof body === 'undefined') {
        return undefined;
    }

    if(typeof body === 'function') {
        return body();
    }

    throw new Error(`Unknown body type ${typeof body}`);
}

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
            expect(response).to.be.instanceOf(ServiceResponse);

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

        it('can send a DELETE message and get a 204 response back with absolute URL', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(204);

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.DELETE).withAbsoluteUrl('https://ds.azurewebsites.net/tables/foo/id');
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.method).to.equal('DELETE');
            expect(req.url).to.equal('https://ds.azurewebsites.net/tables/foo/id');

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

        it('can send a GET message and get a 200 response with content back', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(200,'{"id":"abcd"}');

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.GET, '/tables/foo/id');
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.method).to.equal('GET');
            expect(req.url).to.equal('http://localhost/tables/foo/id');
            expect(getBody(req.body)).to.be.undefined;

            // Check that serviceResponse is constructed properly
            expect(response.content).to.equal('{"id":"abcd"}');
            expect(response.etag).to.be.undefined;
            expect(response.hasContent).to.be.true;
            expect(response.hasValue).to.be.true;
            expect(response.isConflictStatusCode).to.be.false;
            expect(response.isSuccessStatusCode).to.be.true;
            expect(response.statusCode).to.equal(200);
            expect(response.value).to.be.eql({id: 'abcd'});
        });

        it('can send a GET message and get a 200 response with content and etag back', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(200,'{"id":"abcd"}', { 'ETag': '"wumpus"'});

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.GET, '/tables/foo/id');
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.method).to.equal('GET');
            expect(req.url).to.equal('http://localhost/tables/foo/id');
            expect(getBody(req.body)).to.be.undefined;

            // Check that serviceResponse is constructed properly
            expect(response.content).to.equal('{"id":"abcd"}');
            expect(response.etag).to.be.equal('wumpus');
            expect(response.hasContent).to.be.true;
            expect(response.hasValue).to.be.true;
            expect(response.isConflictStatusCode).to.be.false;
            expect(response.isSuccessStatusCode).to.be.true;
            expect(response.statusCode).to.equal(200);
            expect(response.value).to.be.eql({id: 'abcd'});
        });

        it('can send a GET message and get a 200 response with content and weak etag back', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(200,'{"id":"abcd"}', { 'ETag': 'W/"wumpus"'});

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.GET, '/tables/foo/id');
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.method).to.equal('GET');
            expect(req.url).to.equal('http://localhost/tables/foo/id');
            expect(getBody(req.body)).to.be.undefined;

            // Check that serviceResponse is constructed properly
            expect(response.content).to.equal('{"id":"abcd"}');
            expect(response.etag).to.be.equal('W/"wumpus"');
            expect(response.hasContent).to.be.true;
            expect(response.hasValue).to.be.true;
            expect(response.isConflictStatusCode).to.be.false;
            expect(response.isSuccessStatusCode).to.be.true;
            expect(response.statusCode).to.equal(200);
            expect(response.value).to.be.eql({id: 'abcd'});
        });

        it('can send a PUT message and get a 409 response with content back', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(409,'{"id":"abcd"}', { 'ETag': 'W/"wumpus"'});

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.PUT, '/tables/foo/id').withContent(JSON.stringify({id: 'wumpus'}));
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.method).to.equal('PUT');
            expect(req.url).to.equal('http://localhost/tables/foo/id');
            expect(getBody(req.body)).to.equal('{"id":"wumpus"}');

            // Check that serviceResponse is constructed properly
            expect(response.content).to.equal('{"id":"abcd"}');
            expect(response.etag).to.be.equal('W/"wumpus"');
            expect(response.hasContent).to.be.true;
            expect(response.hasValue).to.be.true;
            expect(response.isConflictStatusCode).to.be.true;
            expect(response.isSuccessStatusCode).to.be.false;
            expect(response.statusCode).to.equal(409);
            expect(response.value).to.be.eql({id: 'abcd'});
        });

        it('can send a POST message with headers and context and get a 412 response with content back', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(412,'{"id":"abcd"}', { 'ETag': 'W/"wumpus"'});

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.POST, '/tables/foo')
                .withContent({id: 'wumpus'})
                .withHeader('If-Match', '*');
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.headers.get('if-match')).to.equal('*');
            expect(req.headers.get('content-type')).to.startWith('application/json');
            expect(req.method).to.equal('POST');
            expect(req.url).to.equal('http://localhost/tables/foo');
            expect(getBody(req.body)).to.equal('{"id":"wumpus"}');

            // Check that serviceResponse is constructed properly
            expect(response.content).to.equal('{"id":"abcd"}');
            expect(response.etag).to.be.equal('W/"wumpus"');
            expect(response.hasContent).to.be.true;
            expect(response.hasValue).to.be.true;
            expect(response.headers).to.have.property('etag', 'W/"wumpus"');
            expect(response.isConflictStatusCode).to.be.true;
            expect(response.isSuccessStatusCode).to.be.false;
            expect(response.statusCode).to.equal(412);
            expect(response.value).to.be.eql({id: 'abcd'});
        });

        it('throws a HttpError when requireResponseContent but no content returned', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(204);

            const client = new ServiceHttpClient('http://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.POST, '/tables/foo')
                .withContent({id: 'wumpus'})
                .withHeader('If-Match', '*')
                .requireResponseContent();
            
            try {
                await client.sendServiceRequest(request);
                assert.fail('did not expect to get a valid response');
            } catch (err) {
                expect(err).to.be.instanceOf(HttpError);
                if (err instanceof HttpError) {
                    expect(err.request).to.not.be.undefined;
                    expect(err.response).to.not.be.undefined;
                }
            }
        });

        it('returns a response when requireResponseContent and no content but error', async () => {
            const mock = new MockHttpClient();
            mock.addResponse(412);

            const client = new ServiceHttpClient('https://localhost', { httpClient: mock });
            const request = new ServiceRequest(HttpMethod.POST, '/tables/foo')
                .withContent({id: 'wumpus'})
                .withHeader('If-Match', '*')
                .requireResponseContent();
            const response = await client.sendServiceRequest(request);
            expect(response).to.be.instanceOf(ServiceResponse);

            // Check that request is sent properly
            expect(mock.requests).to.have.lengthOf(1);
            const req = mock.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.headers.get('if-match')).to.equal('*');
            expect(req.headers.get('content-type')).to.startWith('application/json');
            expect(req.method).to.equal('POST');
            expect(req.url).to.equal('https://localhost/tables/foo');

            // Check that serviceResponse is constructed properly
            expect(response.content).to.be.undefined;
            expect(response.etag).to.be.undefined;
            expect(response.hasContent).to.be.false;
            expect(response.hasValue).to.be.false;
            expect(response.isConflictStatusCode).to.be.true;
            expect(response.isSuccessStatusCode).to.be.false;
            expect(response.statusCode).to.equal(412);
            expect(response.value).to.be.undefined;
        });
    });
});
