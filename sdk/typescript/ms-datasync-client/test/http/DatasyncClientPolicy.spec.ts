// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import { datasyncClientPolicy } from '../../src/http/DatasyncClientPolicy';
import { PipelineRequest, createHttpHeaders } from '@azure/core-rest-pipeline';
import { MockHttpClient } from '../helpers/MockHttpClient';

describe('http/DatasyncClientPolicy', () => {
    describe('#name', () => {
        it('has a name', () => {
            const policy = datasyncClientPolicy({});
            expect(policy.name).to.have.length.greaterThan(1);
        });
    });

    describe('#sendRequest', async () => {
        it('sets the protocol version by default', async () => {
            const policy = datasyncClientPolicy({});
            const mockClient = new MockHttpClient();
 
            const request: PipelineRequest = {
                url: 'http://localhost',
                method: 'GET',
                headers: createHttpHeaders(),
                timeout: 0,
                withCredentials: false,
                requestId: '1'
            };
            mockClient.addResponse(204);
            await policy.sendRequest(request, (r) => { return mockClient.sendRequest(r); });

            expect(mockClient.requests).to.have.lengthOf(1);
            const req = mockClient.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
        });

        it('sets the protocol version by option', async () => {
            const policy = datasyncClientPolicy({ apiVersion: '2.1.0' });
            const mockClient = new MockHttpClient();
 
            const request: PipelineRequest = {
                url: 'http://localhost',
                method: 'GET',
                headers: createHttpHeaders(),
                timeout: 0,
                withCredentials: false,
                requestId: '1'
            };
            mockClient.addResponse(204);
            await policy.sendRequest(request, (r) => { return mockClient.sendRequest(r); });

            expect(mockClient.requests).to.have.lengthOf(1);
            const req = mockClient.requests[0];
            expect(req.headers.get('zumo-api-version')).to.equal('2.1.0');
        });

        it('sets the internal user-agent when user-agent is set', async () => {
            const policy = datasyncClientPolicy({ apiVersion: '2.1.0' });
            const mockClient = new MockHttpClient();
 
            const headers = createHttpHeaders();
            headers.set('user-agent', 'test-wumpus');
            const request: PipelineRequest = {
                url: 'http://localhost',
                method: 'GET',
                headers: headers,
                timeout: 0,
                withCredentials: false,
                requestId: '1'
            };
            mockClient.addResponse(204);
            await policy.sendRequest(request, (r) => { return mockClient.sendRequest(r); });

            expect(mockClient.requests).to.have.lengthOf(1);
            const req = mockClient.requests[0];
            expect(req.headers.get('x-zumo-version')).to.equal('test-wumpus');
        });

        it('does not set the internal user-agent when user-agent is unset', async () => {
            const policy = datasyncClientPolicy({ apiVersion: '2.1.0' });
            const mockClient = new MockHttpClient();
 
            const headers = createHttpHeaders();
            headers.delete('user-agent');
            const request: PipelineRequest = {
                url: 'http://localhost',
                method: 'GET',
                headers: headers,
                timeout: 0,
                withCredentials: false,
                requestId: '1'
            };
            mockClient.addResponse(204);
            await policy.sendRequest(request, (r) => { return mockClient.sendRequest(r); });

            expect(mockClient.requests).to.have.lengthOf(1);
            const req = mockClient.requests[0];
            expect(req.headers.get('x-zumo-version')).to.be.undefined;
        });
    });
});