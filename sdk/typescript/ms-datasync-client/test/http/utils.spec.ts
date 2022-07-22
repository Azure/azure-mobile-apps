// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as http from '../../src/http';
import { createPipeline, getErrorMessageFromContent, getUserAgent, getRequestMessage } from '../../src/http/utils';

class MockDelegatingHandler extends http.DelegatingHandler { }

describe('src/client/utils', () => {
    describe('#createPipeline', () => {
        it('works with no handlers', () => {
            const args: Array<http.HttpMessageHandler> = [];
            const pipeline = createPipeline(args);

            assert.instanceOf(pipeline, http.HttpClientHandler);
        });

        it('works with a client handler', () => {
            const c = new http.AxiosClientHandler();
            const args: Array<http.HttpMessageHandler> = [ c ];
            const pipeline = createPipeline(args);

            assert.equal(pipeline, c);
        });

        it('works with a delegating handler', () => {
            const b = new MockDelegatingHandler();
            const args: Array<http.HttpMessageHandler> = [ b ];
            const pipeline = createPipeline(args);

            assert.equal(pipeline, b);
            assert.instanceOf(b.innerHandler, http.HttpClientHandler);
        });

        it('works with a delegating handler and client handler', () => {
            const b = new MockDelegatingHandler();
            const c = new http.AxiosClientHandler();
            const args: Array<http.HttpMessageHandler> = [ b, c ];
            const pipeline = createPipeline(args);

            assert.equal(pipeline, b);
            assert.equal(b.innerHandler, c);
        });

        it('works with 2 delegating handlers', () => {
            const a = new MockDelegatingHandler();
            const b = new MockDelegatingHandler();
            const args: Array<http.HttpMessageHandler> = [ a, b ];
            const pipeline = createPipeline(args);

            assert.equal(pipeline, a);
            assert.equal(a.innerHandler, b);
            assert.instanceOf(b.innerHandler, http.HttpClientHandler);
        });

        it('works with 2 delegating handlers and a client handler', () => {
            const a = new MockDelegatingHandler();
            const b = new MockDelegatingHandler();
            const c = new http.AxiosClientHandler();
            const args: Array<http.HttpMessageHandler> = [ a, b, c ];
            const pipeline = createPipeline(args);

            assert.equal(pipeline, a);
            assert.equal(a.innerHandler, b);
            assert.equal(b.innerHandler, c);
        });

        it('throws with a client handler then a delegating handler', () => {
            const a = new MockDelegatingHandler();
            const c = new http.AxiosClientHandler();
            const args: Array<http.HttpMessageHandler> = [ c, a ];
            assert.throws(() => {  createPipeline(args); })

        });

        it('throws with a client handler then 2 delegating handlers', () => {
            const a = new MockDelegatingHandler();
            const b = new MockDelegatingHandler();
            const c = new http.AxiosClientHandler();
            const args: Array<http.HttpMessageHandler> = [ c, a, b ];
            assert.throws(() => {  createPipeline(args); })
        });

        it('throws with a client handler between delegating handlers', () => {
            const a = new MockDelegatingHandler();
            const b = new MockDelegatingHandler();
            const c = new http.AxiosClientHandler();
            const args: Array<http.HttpMessageHandler> = [ a, c, b ];
            assert.throws(() => {  createPipeline(args); })
        });
    });

    describe("#getErrorMessageFromContent", () => {
        it('returns undefined for empty string', () => { assert.isUndefined(getErrorMessageFromContent('')); });
        it('returns undefined for white space', () => { assert.isUndefined(getErrorMessageFromContent('   ')); });
        it('returns undefined for empty object', () => { assert.isUndefined(getErrorMessageFromContent('{}')); });
        it('returns string for non-JSON', () => { assert.equal(getErrorMessageFromContent('foo'), 'foo'); });
        it('returns error message for standard-error', () => { assert.equal(getErrorMessageFromContent('{"error":"foo"}'), 'foo'); });
        it('returns error message for standard-description', () => { assert.equal(getErrorMessageFromContent('{"description":"foo"}'), 'foo'); });
        it('returns undefined for nonstandard object', () => { assert.isUndefined(getErrorMessageFromContent('{"code":1234}')); });
    });

    describe("#getRequestMessage", () => {
        it('sets the url when pathAndQuery is absolute', () => {
            const serviceRequest = new http.ServiceRequest().withPathAndQuery('http://localhost/tables/foo');
            const request = getRequestMessage(serviceRequest, new URL('http://ds.azurewebsites.net'));
            assert.equal('http://localhost/tables/foo', request.requestUri.href);
        });

        it('sets the url when pathAndQuery is relative with leading slash', () => {
            const serviceRequest = new http.ServiceRequest().withPathAndQuery('/tables/foo');
            const request = getRequestMessage(serviceRequest, new URL('http://ds.azurewebsites.net'));
            assert.equal('http://ds.azurewebsites.net/tables/foo', request.requestUri.href);
        });

        it('sets the url when pathAndQuery is relative without leading slash', () => {
            const serviceRequest = new http.ServiceRequest().withPathAndQuery('tables/foo');
            const request = getRequestMessage(serviceRequest, new URL('http://ds.azurewebsites.net'));
            assert.equal('http://ds.azurewebsites.net/tables/foo', request.requestUri.href);
        });
    });

    describe('#userAgent', () => {
        it('returns a valid user agent', () => {
            assert.isTrue(getUserAgent().startsWith('Datasync/'));
        });
    });
});