// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as http from '../../src/http';
import { createPipeline, getUserAgent } from '../../src/http/utils';

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

    describe('#userAgent', () => {
        it('returns a valid user agent', () => {
            assert.isTrue(getUserAgent().startsWith('Datasync/'));
        });
    });
});