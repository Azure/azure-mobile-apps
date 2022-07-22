// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import { ServiceClient } from '../../src/http';

describe('src/http/serviceClient', () => {
    describe('#ctor', () => {
        it('throws for invalid endpoint - http domain', () => { 
            const endpoint = new URL('http://ds.azurewebistes.net');
            assert.throws(() => { new ServiceClient(endpoint, {}); });
        });
        it('throws for invalid endpoint - http IPv4', () => { 
            const endpoint = new URL('http://8.8.8.8');
            assert.throws(() => { new ServiceClient(endpoint, {}); });
        });
        it('throws for invalid endpoint - http IPv6', () => { 
            const endpoint = new URL('http://[2001:0db8:85a3:0000:0000:8a2e:0370:7334]');
            assert.throws(() => { new ServiceClient(endpoint, {}); });
        });
        it('sets the endpoint from the ctor', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, {});
            assert.equal(client.endpoint.href, 'https://datasync.azurewebsites.net/');
        });
        it('sets the installationId from the ctor when not specified', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, {});
            assert.equal(client.installationId, '');
        });
        it('sets the installationId from the ctor when specified', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { installationId: 'abc' });
            assert.equal(client.installationId, 'abc');
        });
        it('sets the default headers in the ctor (UA=no,IID=no)', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { });
            assert.equal(client.defaultHeaders['ZUMO-API-VERSION'], '3.0.0');
            assert.isTrue(client.defaultHeaders['X-ZUMO-VERSION'].startsWith('Datasync/'));
            assert.isUndefined(client.defaultHeaders['X-ZUMO-INSTALLATION-ID']);
        });
        it('sets the default headers in the ctor (UA=blank,IID=no)', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { userAgent: '' });
            assert.equal(client.defaultHeaders['ZUMO-API-VERSION'], '3.0.0');
            assert.isUndefined(client.defaultHeaders['X-ZUMO-VERSION']);
            assert.isUndefined(client.defaultHeaders['X-ZUMO-INSTALLATION-ID']);
        });
        it('sets the default headers in the ctor (UA=yes,IID=no)', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { userAgent: 'yes' });
            assert.equal(client.defaultHeaders['ZUMO-API-VERSION'], '3.0.0');
            assert.equal(client.defaultHeaders['X-ZUMO-VERSION'], 'yes');
            assert.isUndefined(client.defaultHeaders['X-ZUMO-INSTALLATION-ID']);
        });
        it('sets the default headers in the ctor (UA=no,IID=yes)', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { installationId: 'foo' });
            assert.equal(client.defaultHeaders['ZUMO-API-VERSION'], '3.0.0');
            assert.isTrue(client.defaultHeaders['X-ZUMO-VERSION'].startsWith('Datasync/'));
            assert.equal(client.defaultHeaders['X-ZUMO-INSTALLATION-ID'], 'foo');
        });
        it('sets the default headers in the ctor (UA=blank,IID=yes)', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { userAgent: '',  installationId: 'foo' });
            assert.equal(client.defaultHeaders['ZUMO-API-VERSION'], '3.0.0');
            assert.isUndefined(client.defaultHeaders['X-ZUMO-VERSION']);
            assert.equal(client.defaultHeaders['X-ZUMO-INSTALLATION-ID'], 'foo');
        });
        it('sets the default headers in the ctor (UA=yes,IID=yes)', () => {
            const endpoint = new URL('https://datasync.azurewebsites.net');
            const client = new ServiceClient(endpoint, { userAgent: 'yes',  installationId: 'foo' });
            assert.equal(client.defaultHeaders['ZUMO-API-VERSION'], '3.0.0');
            assert.equal(client.defaultHeaders['X-ZUMO-VERSION'], 'yes');
            assert.equal(client.defaultHeaders['X-ZUMO-INSTALLATION-ID'], 'foo');
        });
    });
});