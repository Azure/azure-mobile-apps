// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    corsModule = require('../../src/cors');

describe('azure-mobile-apps.cors.origin', function() {
    it('allows protocol/port variations on an allowed hostname', function() {
        var cors = corsModule({
            hostnames: [{
                host: 'some.site.example.com'
            }, {
                host: 'wildcard.*.example.com'
            }]
        });

        expect(cors.isAllowedOrigin('http://some.site.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('http://wildcard.any-thing.here.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('https://some.site.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('https://wildcard.any-thing.here.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('http://some.site.example.com:12345')).to.be.true;
        expect(cors.isAllowedOrigin('http://wildcard.any-thing.here.example.com:12345')).to.be.true;
        expect(cors.isAllowedOrigin('https://some.site.example.com:12345')).to.be.true;
        expect(cors.isAllowedOrigin('https://wildcard.any-thing.here.example.com:12345')).to.be.true;
        expect(cors.isAllowedOrigin('ms-appx-web://wildcard.whatever.here.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('ms-appx-web://some.site.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('ms-appx-web://wildcard.foo.bar.here.example.com:12345')).to.be.true;
        expect(cors.isAllowedOrigin('ms-appx-web://some.site.example.com:54321')).to.be.true;
    });

    it('allows and ignores embedded username/passwords in an allowed hostname', function() {
        var cors = corsModule({
            hostnames: [{
                host: 'some.site.example.com'
            }, {
                host: 'wildcard.*.example.com'
            }]
        });
        expect(cors.isAllowedOrigin('http://user:pass@some.site.example.com')).to.be.true;
        expect(cors.isAllowedOrigin('http://user:pass@wildcard.any-thing.here.example.com')).to.be.true;
    });

    it('disallows disallowed hostnames', function() {
        var cors = corsModule({
            hostnames: [{
                host: 'some.site.example.com'
            }, {
                host: 'wildcard.*.example.com'
            }, {
                host: 'null'
            }]
        });

        expect(cors.isAllowedOrigin('http://subsite.some.site.example.com')).to.be.false; // Subdomain
        expect(cors.isAllowedOrigin('http://site.example.com')).to.be.false; // Parent domain
        expect(cors.isAllowedOrigin('http://subsite.some.site.example.com.tld')).to.be.false; // Strange suffix
        expect(cors.isAllowedOrigin('http://wildcard-mismatch.example.com')).to.be.false; // Close, but not close enough
        expect(cors.isAllowedOrigin('http://wildcard.?.example.com')).to.be.false; // Illegal char in wildcard
        expect(cors.isAllowedOrigin('http://wildcard.my\nvalue.example.com')).to.be.false; // Linebreak in wildcard
    });

    it('allows null filesystem origins when null is specified', function() {
        // This whitelist allows any legal http/https hostname
        var cors = corsModule({
            hostnames: [{
                host: 'null'
            }]
        });

        expect(cors.isAllowedOrigin('null')).to.be.true; // Most browsers send this for filesystem origins
        expect(cors.isAllowedOrigin('file://')).to.be.false; // Safari 5 send this for filesystem origins
    });

    it('allows only specific origins when * is specified', function() {
        // This whitelist allows any legal http/https hostname
        var cors = corsModule({
            hostnames: [{
                host: '*'
            }]
        });

        expect(cors.isAllowedOrigin('http://foo.com')).to.be.true;
        expect(cors.isAllowedOrigin('https://bar.com')).to.be.true;
        expect(cors.isAllowedOrigin('ms-appx-web://mysillyapp.com')).to.be.true; // iframe in Windows Modern Apps uses this
        expect(cors.isAllowedOrigin('no.protocol.example.com')).to.be.false;
        expect(cors.isAllowedOrigin('unknown://example.com')).to.be.false;
        expect(cors.isAllowedOrigin('null')).to.be.false; // Most browsers send this for filesystem origins
        expect(cors.isAllowedOrigin('file://')).to.be.false; // Safari 5 send this for filesystem origins
    });


    it('allows a trailing slash in an origin, but no other path', function() {
        // This whitelist allows any legal http/https hostname
        var cors = corsModule({
            hostnames: [{
                host: '*'
            }]
        });

        expect(cors.isAllowedOrigin('http://localhost/')).to.be.true;
        expect(cors.isAllowedOrigin('https://localhost:12345/')).to.be.true;
        expect(cors.isAllowedOrigin('http://localhost/path')).to.be.false;
        expect(cors.isAllowedOrigin('https://localhost:12345/path?and=query')).to.be.false;
    });

    it('does not allow anything when whitelist is explicitly empty', function() {
        var cors = corsModule({
            hostnames: []
        });
        expect(cors.isAllowedOrigin('http://localhost')).to.be.false;
    });
});
