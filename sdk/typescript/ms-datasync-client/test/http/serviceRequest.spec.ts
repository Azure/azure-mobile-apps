// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import { HttpMethod, ServiceRequest } from '../../src/http';

describe('src/http/serviceRequest', () => {
    describe('#constructor', () => {
        it('has default content', () => {
            const request = new ServiceRequest();
            expect(request.content).to.be.undefined;
        });

        it('does not ensure response content', () => {
            const request = new ServiceRequest();
            expect(request.ensureResponseContent).to.be.false;
        });

        it('has default headers', () => {
            const request = new ServiceRequest();
            expect(request.headers).to.eql({});
        });

        it('uses GET method', () => {
            const request = new ServiceRequest();
            expect(request.method).to.equal(HttpMethod.Get);
        });

        it('has blank path and query', () => {
            const request = new ServiceRequest();
            expect(request.pathAndQuery).to.equal('');
        });

        it('can handle complex chaining', () => {
            const request = new ServiceRequest().requireResponseContent().withMethod(HttpMethod.Post).withPathAndQuery('/tables/foo').withContent('{ "id": "1234" }').withHeaders({ 'If-None-Match': '*' });
            expect(request.ensureResponseContent).to.be.true;
            expect(request.content).to.equal('{ "id": "1234" }');
            expect(request.headers).to.eql({ 'If-None-Match': '*' });
            expect(request.method).to.equal(HttpMethod.Post);
            expect(request.pathAndQuery).to.equal('/tables/foo');
        });
    });

    describe('#requireResponseContent', () => {
        it('sets property when chained', () => {
            const request = new ServiceRequest().requireResponseContent();
            expect(request.ensureResponseContent).to.be.true;
        });
    });

    describe('#withContent', () => {
        it('sets property when chained', () => {
            const request = new ServiceRequest().withContent('foo');
            expect(request.content).to.equal('foo');
        });
    });

    describe('#withHeaders', () => {
        it('sets property when chained', () => {
            const request = new ServiceRequest().withHeaders({ 'X-ZUMO-REQUEST': '1234' });
            expect(request.headers).to.eql({ 'X-ZUMO-REQUEST': '1234' });
        });
    });

    describe('#withMethod', () => {
        it('sets property when chained', () => {
            const request = new ServiceRequest().withMethod(HttpMethod.Delete);
            expect(request.method).to.equal(HttpMethod.Delete);
        });
    });

    describe('#withPathAndQuery', () => {
        it('sets property when chained', () => {
            const request = new ServiceRequest().withPathAndQuery('/foo/bar');
            expect(request.pathAndQuery).to.equal('/foo/bar');
        });
    });
});