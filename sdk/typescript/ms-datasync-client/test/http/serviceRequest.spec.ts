// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect, use } from 'chai';
import chaiString from 'chai-string';
import { ArgumentError } from '../../src';
import { HttpHeaders, HttpMethod, ServiceRequest } from '../../src/http';

use(chaiString);

describe('http/ServiceRequest', () => {
    describe('#constructor', () => {
        it('sets default values', () => {
            const req = new ServiceRequest();
            expect(req.content).to.be.undefined;
            expect(req.ensureResponseContent).to.be.false;
            expect(req.headers).to.eql({});
            expect(req.method).to.equal(HttpMethod.GET);
            expect(req.path).to.equal('');
            expect(req.queryString).to.be.undefined;
        });

        it('can set method to DELETE', () => {
            const req = new ServiceRequest(HttpMethod.DELETE);
            expect(req.method).to.equal(HttpMethod.DELETE);
        });

        it('can set method to GET', () => {
            const req = new ServiceRequest(HttpMethod.GET);
            expect(req.method).to.equal(HttpMethod.GET);
        });

        it('can set method to POST', () => {
            const req = new ServiceRequest(HttpMethod.POST);
            expect(req.method).to.equal(HttpMethod.POST);
        });

        it('can set method to PUT', () => {
            const req = new ServiceRequest(HttpMethod.PUT);
            expect(req.method).to.equal(HttpMethod.PUT);
        });

        it('can set method to QUERY', () => {
            const req = new ServiceRequest(HttpMethod.QUERY);
            expect(req.method).to.equal(HttpMethod.QUERY);
        });

        it('throws when setting an invalid method', () => {
            const value: HttpMethod = 90;
            expect(() => { new ServiceRequest(value); }).to.throw(ArgumentError);
        });

        it('can set a path', () => {
            const req = new ServiceRequest(HttpMethod.GET, '/tables/todoitem');
            expect(req.path).to.equal('/tables/todoitem');
        });

        it('can set an empty path', () => {
            const req = new ServiceRequest(HttpMethod.GET, '');
            expect(req.path).to.equal('');
        });

        it('throws on an invalid path', () => {
            expect(() => { new ServiceRequest(HttpMethod.GET, 'foo'); }).to.throw(ArgumentError);
        });
    });

    describe('#removeContent', () => {
        it('removes content', () => {
            const req = new ServiceRequest().withContent('abcd');
            req.removeContent();
            expect(req.content).to.be.undefined;
        });
    });

    describe('#removeHeader', () => {
        it('removes a header that exists', () => {
            const req = new ServiceRequest().withHeaders({ 'X-HeaderName': 'foo', 'X-Barf': 'Barf' });
            expect(req.removeHeader('X-HeaderName').headers).to.eql({'X-Barf': 'Barf'});
        });

        it('handles a header that does not exist', () => {
            const req = new ServiceRequest().withHeaders({ 'X-Barf': 'Barf' });
            expect(req.removeHeader('X-HeaderName').headers).to.eql({'X-Barf': 'Barf'});
        });

        it('throws if the header name is invalid', () => {
            const req = new ServiceRequest().withHeaders({ 'X-HeaderName': 'foo' });
            expect(() => { req.removeHeader('-X-'); }).to.throw;
        });
    });

    describe('#removeQueryString', () => {
        it('removes the query string', () => {
            const req = new ServiceRequest().withQueryString('foo=bar');
            req.removeQueryString();
            expect(req.queryString).to.be.undefined;
        });
    });
    
    describe('#requireResponseContent', () => {
        it('sets ensureResponseContent to true when not specified', () => {
            const req = new ServiceRequest().requireResponseContent();
            expect(req.ensureResponseContent).to.be.true;
        });

        it('sets ensureResponseContent to true when true', () => {
            const req = new ServiceRequest().requireResponseContent(true);
            expect(req.ensureResponseContent).to.be.true;
        });

        it('sets ensureResponseContent to false when false', () => {
            const req = new ServiceRequest().requireResponseContent(false);
            expect(req.ensureResponseContent).to.be.false;
        });
    });

    describe('#withAbsoluteUrl', () => {
        it('handles http (string)', () => {
            const req = new ServiceRequest().withAbsoluteUrl('http://localhost');
            expect(req.path).to.equal('http://localhost/');
        });
        it('handles https (string)', () => {
            const req = new ServiceRequest().withAbsoluteUrl('https://localhost');
            expect(req.path).to.equal('https://localhost/');
        });
        it('throws on non-http (string)', () => {
            const req = new ServiceRequest();
            expect(() => req.withAbsoluteUrl('file:///foo')).to.throw;
        });

        it('handles http (URL)', () => {
            const req = new ServiceRequest().withAbsoluteUrl(new URL('http://localhost'));
            expect(req.path).to.equal('http://localhost/');
        });
        it('handles https (URL)', () => {
            const req = new ServiceRequest().withAbsoluteUrl(new URL('https://localhost'));
            expect(req.path).to.equal('https://localhost/');
        });
        it('throws on non-http (URL)', () => {
            const req = new ServiceRequest();
            expect(() => req.withAbsoluteUrl(new URL('file:///foo'))).to.throw;
        });
    });

    describe('#withContent', () => {
        it('clears content when undefined', () => {
            const req = new ServiceRequest().withContent('abcd');
            expect(req.content).to.equal('abcd');
            req.withContent(undefined);
            expect(req.content).to.be.undefined;
        });

        it('sets a string value', () => {
            const req = new ServiceRequest().withContent('abcd');
            expect(req.content).to.equal('abcd');
        });

        it('sets an object', () => {
            const content = { a: 'foo' };
            const req = new ServiceRequest().withContent(content);
            expect(req.content).to.equal('{"a":"foo"}');
            expect(req.headers['Content-Type']).to.startWith('application/json');
        });

        it('sets an array', () => {
            const content = [ 'a', 'b' ];
            const req = new ServiceRequest().withContent(content);
            expect(req.content).to.equal('["a","b"]');
            expect(req.headers['Content-Type']).to.startWith('application/json');
        });
    });

    describe('#withHeader', () => {
        it('adds a header', () => {
            const req = new ServiceRequest();
            req.withHeader('X-HeaderName', 'foo');
            req.withHeader('Content-Type', 'application/json');
            expect(req.headers).to.eql({'X-HeaderName': 'foo', 'Content-Type': 'application/json'});
        });

        it('can replace a header', () => {
            const req = new ServiceRequest();
            req.withHeader('X-HeaderName', 'foo');
            req.withHeader('X-HeaderName', 'bar');
            expect(req.headers).to.eql({'X-HeaderName': 'bar'});
        });

        it('throws on blank header name', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('', 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on short header name', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('a', 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on long header name', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader(new Array(100).join('a'), 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on header name with numeric start', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('0abc', 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on header name with invalid start', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('-X', 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on header name with invalid character', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('X-$-Foo', 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on header name with invalid ending', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('X-Foo-', 'foo'); }).to.throw(ArgumentError);
        });

        it('throws on empty header value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('X-Header', ''); }).to.throw(ArgumentError);
        });

        it('throws on too long header value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('X-Header', new Array(300).join('a')); }).to.throw(ArgumentError);
        });

        it('throws on non-printable header-value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('X-Header', '\u000A'); }).to.throw(ArgumentError);
        });

        it('throws on empty header value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeader('X-Header', ''); }).to.throw(ArgumentError);
        });
    });

    describe('#withHeaders', () => {
        it('can clear the headers', () => {
            const req = new ServiceRequest();
            expect(req.withHeaders({}).headers).to.eql({});
        });

        it('can set the headers', () => {
            const req = new ServiceRequest();
            expect(req.withHeaders({'Content-Type': 'application/json'}).headers).to.eql({'Content-Type': 'application/json'});
        });

        it('throws on short header name', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ a: 'foo' }); }).to.throw(ArgumentError);
        });

        it('throws on long header name', () => {
            const req = new ServiceRequest();
            const headers: HttpHeaders = {};
            headers[new Array(100).join('a')] = 'foo';
            expect(() => { req.withHeaders(headers); }).to.throw(ArgumentError);
        });

        it('throws on header name with numeric start', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ '0abc': 'foo' }); }).to.throw(ArgumentError);
        });

        it('throws on header name with invalid start', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ '-X': 'foo' }); }).to.throw(ArgumentError);
        });

        it('throws on header name with invalid character', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ 'X-$-Foo': 'foo' }); }).to.throw(ArgumentError);
        });

        it('throws on header name with invalid ending', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ 'X-Foo-': 'foo' }); }).to.throw(ArgumentError);
        });

        it('throws on empty header value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ 'X-Header': '' }); }).to.throw(ArgumentError);
        });

        it('throws on too long header value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ 'X-Header': new Array(300).join('a') }); }).to.throw(ArgumentError);
        });

        it('throws on non-printable header-value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ 'X-Header': '\u000A' }); }).to.throw(ArgumentError);
        });

        it('throws on empty header value', () => {
            const req = new ServiceRequest();
            expect(() => { req.withHeaders({ 'X-Header': '' }); }).to.throw(ArgumentError);
        });
    });

    describe('#withMethod', () => {
        it('can set method to DELETE', () => {
            const req = new ServiceRequest().withMethod(HttpMethod.DELETE);
            expect(req.method).to.equal(HttpMethod.DELETE);
        });

        it('can set method to GET', () => {
            const req = new ServiceRequest().withMethod(HttpMethod.GET);
            expect(req.method).to.equal(HttpMethod.GET);
        });

        it('can set method to POST', () => {
            const req = new ServiceRequest().withMethod(HttpMethod.POST);
            expect(req.method).to.equal(HttpMethod.POST);
        });

        it('can set method to PUT', () => {
            const req = new ServiceRequest().withMethod(HttpMethod.PUT);
            expect(req.method).to.equal(HttpMethod.PUT);
        });

        it('can set method to QUERY', () => {
            const req = new ServiceRequest().withMethod(HttpMethod.QUERY);
            expect(req.method).to.equal(HttpMethod.QUERY);
        });

        it('throws when setting an invalid method', () => {
            const value: HttpMethod = 90;
            expect(() => { new ServiceRequest().withMethod(value); }).to.throw(ArgumentError);
        });
    });

    describe('#withPath', () => {
        it('can set to empty path', () => {
            const req = new ServiceRequest().withPath('');
            expect(req.path).to.equal('');
        });

        it('can set a good path', () => {
            const req = new ServiceRequest().withPath('/tables/foo');
            expect(req.path).to.equal('/tables/foo');
        });

        it('throws for invalid paths', () => {
            const values: Array<string> = [
                '///',              // zero length segments
                'foo',              // Bare word - no leading slash
                '/foo\u000Abar',    // invalid character
                'foo/bar',          // Another bare word but with feeling
                '/foo/bar\u000A',   // Another embedded character
                '/[]',              // square brackets
                '/<foo>',           // pointy brackets
                '/*foo',            // star
                '/foo?',            // question mark
            ];
            const req = new ServiceRequest();

            for (const value of values) {
                expect(() => { req.withPath(value); }).to.throw(ArgumentError);
            }
        });
    });

    describe('#withQueryString', () => {
        it('sets to undefined when blank', () => {
            const req = new ServiceRequest().withQueryString('abcd');
            expect(req.withQueryString('').queryString).to.be.undefined;
        });

        it('sets to undefined when question mark', () => {
            const req = new ServiceRequest().withQueryString('abcd');
            expect(req.withQueryString('?').queryString).to.be.undefined;
        });

        it('trims content', () => {
            const req = new ServiceRequest().withQueryString('abcd');
            expect(req.withQueryString('?abc=foo   ').queryString).to.equal('abc=foo');
        });
    });

    describe('#withVersionHeader', () => {
        it('sets If-Match header', () => {
            const req = new ServiceRequest().withHeaders({ 'ZUMO-API-VERSION': '3.0.0' });
            expect(req.withVersionHeader('abcd').headers).to.eql({ 'ZUMO-API-VERSION': '3.0.0', 'If-Match': '"abcd"'});
        });

        it('skips if blank', () => {
            const req = new ServiceRequest().withHeaders({ 'ZUMO-API-VERSION': '3.0.0' });
            expect(req.withVersionHeader('').headers).to.eql({ 'ZUMO-API-VERSION': '3.0.0' });
        });

        it('skips if undefined', () => {
            const req = new ServiceRequest().withHeaders({ 'ZUMO-API-VERSION': '3.0.0' });
            expect(req.withVersionHeader(undefined).headers).to.eql({ 'ZUMO-API-VERSION': '3.0.0' });
        });
    });
});
