// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import { ArgumentError } from '../../src/errors';
import * as validate from '../../src/http/validate';

describe('http/validate', () => {
    describe('#isLocalNetwork', () => {
        it('returns true for localhost', () => { expect(validate.isLocalNetwork('localhost')).to.be.true; });
        it('returns true for .local domain', () => { expect(validate.isLocalNetwork('myhost.local')).to.be.true; });
        it('returns true for IPv4 loopback', () => { expect(validate.isLocalNetwork('127.0.0.1')).to.be.true; });
        it('returns true for IPv6 loopback', () => { expect(validate.isLocalNetwork('::1')).to.be.true; });
        it('returns true for wrapped IPv6 loopback', () => { expect(validate.isLocalNetwork('[::1]')).to.be.true; });
        it('returns true for 10.x private net', () => { expect(validate.isLocalNetwork('10.100.8.1')).to.be.true; });
        it('returns true for 172.16.x private net', () => { expect(validate.isLocalNetwork('172.17.200.50')).to.be.true; });
        it('returns true for 192.168.x private net', () => { expect(validate.isLocalNetwork('192.168.0.2')).to.be.true; });

        it('returns false for custom domain', () => { expect(validate.isLocalNetwork('mydomain.com'), 'mydomain.com').to.be.false; });
        it('returns false for custom domain', () => { expect(validate.isLocalNetwork('ds.azurewebsites.net'), 'ds.azurewebsites.net').to.be.false; });
        it('returns false for custom domain', () => { expect(validate.isLocalNetwork('4.4.4.4'), 'Non-local IPv4').to.be.false; });
        it('returns false for custom domain', () => { expect(validate.isLocalNetwork('2001:0db8:85a3:0000:0000:8a2e:0370:7334'), 'Non-local IPv6').to.be.false; });
    });

    describe('#isAbsoluteHttpEndpoint', () => {
        it('throws on (string) invalid URI', () => {
            const sut = 'some-bogus!uri';
            expect(() => { validate.isAbsoluteHttpEndpoint(sut, 'sut'); }).to.throw(ArgumentError);
        });

        it('throws on (string) http:non-local', () => {
            const sut = 'http://ds.azurewebsites.net';
            expect(() => { validate.isAbsoluteHttpEndpoint(sut, 'sut'); }).to.throw(ArgumentError);
        });

        it('throws on (string) non-http', () => {
            const sut = 'file:///foo';
            expect(() => { validate.isAbsoluteHttpEndpoint(sut, 'sut'); }).to.throw(ArgumentError);
        });

        it('passes (string) http-localdomain', () => {
            const sut = 'http://localhost';
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('http://localhost/');
        });

        it('passes (string) https-remotedomain', () => {
            const sut = 'https://dz.azurewebsites.net';
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('https://dz.azurewebsites.net/');
        });

        it('strips fragment (string)', () => {
            const sut = 'https://dz.azurewebsites.net#fragment';
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('https://dz.azurewebsites.net/');
        });

        it('strips query string (string)', () => {
            const sut = 'https://dz.azurewebsites.net?search';
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('https://dz.azurewebsites.net/');
        });

        it('throws on (URL) http:non-local', () => {
            const sut = new URL('http://ds.azurewebsites.net');
            expect(() => { validate.isAbsoluteHttpEndpoint(sut, 'sut'); }).to.throw(ArgumentError);
        });

        it('throws on (URL) non-http', () => {
            const sut = new URL('file:///foo');
            expect(() => { validate.isAbsoluteHttpEndpoint(sut, 'sut'); }).to.throw(ArgumentError);
        });

        it('passes (URL) http-localdomain', () => {
            const sut = new URL('http://localhost');
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('http://localhost/');
        });

        it('passes (URL) https-remotedomain', () => {
            const sut = new URL('https://dz.azurewebsites.net');
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('https://dz.azurewebsites.net/');
        });

        it('strips fragment (URL)', () => {
            const sut = new URL('https://dz.azurewebsites.net#fragment');
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('https://dz.azurewebsites.net/');
        });

        it('strips query string (URL)', () => {
            const sut = new URL('https://dz.azurewebsites.net?search');
            expect(validate.isAbsoluteHttpEndpoint(sut, 'sut').href).to.equal('https://dz.azurewebsites.net/');
        });
    });

    describe('#isRelativePath', () => {
        it('passes empty path', () => {
            expect(() => { validate.isRelativePath('', 'foo'); }).to.not.throw;
        });

        it('passes valid paths', () => {
            const values: Array<string> = [
                '/tables/todoitem'
            ];

            for (const value of values) {
                expect(() => { validate.isRelativePath(value, 'foo'); }).to.not.throw;
            }
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

            for (const value of values) {
                expect(() => { validate.isRelativePath(value, 'foo'); }, `Value = ${JSON.stringify(value)}`).to.throw(ArgumentError);
            }
        });
    });

    describe('#isValidHeaderName', () => {
        it('passes valid header names', () => {
            const values: Array<string> = [
                'Content-Type',
                'Accept',
                'X_ZUMO_INSTALLATION_ID',
                'ZUMO-API-VERSION',
                'If-Match',
                'If-None-Match',
                'If-Modified-Since',
                'If-Unmodified-Since',
                'Protocol-Version-3',
                'W3C-Recommendation'
            ];

            for (const value of values) {
                validate.isValidHeaderName(value, 'foo');
            }
        });

        it('throws for invalid header names', () => {
            const values: Array<string> = [
                '',                         // blank
                'a',                        // too short
                '-X-',                      // doesn't start with alpha
                '9X-Foo',                   // doesn't start with alpha
                'X-Foo-',                   // doesn't end with alphanumeric
                'X-\u000A',                 // control characters
                new Array(100).join('a'),   // too long
            ];

            for (const value of values) {
                expect(() => { validate.isValidHeaderName(value, 'foo'); }).to.throw(ArgumentError);
            }
        });
    });

    describe('#isValidHeaderValue', () => {
        it('passes valid header values', () => {
            const values: Array<string> = [
                'a',
                'W/"123456"',
                'application/json; charset=utf8',
            ];

            for (const value of values) {
                validate.isValidHeaderValue(value, 'value');
            }
        });
        
        it('throws for invalid header values', () => {
            const values: Array<string> = [
                '',                         // blank
                'X-\u000A',                 // control characters
                new Array(300).join('a'),   // too long
            ];

            for (const value of values) {
                expect(() => { validate.isValidHeaderValue(value, 'foo'); }).to.throw(ArgumentError);
            }
        });
    });
});