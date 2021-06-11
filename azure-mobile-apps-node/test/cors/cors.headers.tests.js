// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
	corsModule = require('../../src/cors');

var accessControlAllowOriginHeader = 'Access-Control-Allow-Origin',
	accessControlAllowMethodsHeader = 'Access-Control-Allow-Methods',
	accessControlAllowHeadersHeader = 'Access-Control-Allow-Headers',
	accessControlMaxAgeHeader = 'Access-Control-Max-Age',
	expectedAllowedMethods = 'GET, PUT, PATCH, POST, DELETE, OPTIONS';

describe('azure-mobile-apps.cors.headers', function() {
	it('Returns no headers if the client did not supply an origin', function() {
		var cors = corsModule();
		expect(Object.keys(cors.getHeaders()).length === 0).to.be.true;
	});

	it('Echoes back default max age on preflight requests', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});

		expect(cors.getHeaders('https://blah.example.com:234', undefined, 'OPTIONS')[accessControlMaxAgeHeader]).to.equal(300);
		expect(cors.getHeaders('https://blah.example.com:234')[accessControlMaxAgeHeader]).equal(undefined);
	});

	it('Echoes back supplied max age on preflight requests', function() {
		var cors = corsModule({
			maxAge: 60000,
			hostnames: [{
				host: '*.example.com'
			}]
		});

		expect(cors.getHeaders('https://blah.example.com:234', undefined, 'OPTIONS')[accessControlMaxAgeHeader]).to.equal(60000);
		expect(cors.getHeaders('https://blah.example.com:234')[accessControlMaxAgeHeader]).to.equal(undefined);
	});

	it('Echoes back supplied origin if matches whitelist', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});

		expect(cors.getHeaders('https://blah.example.com:234')[accessControlAllowOriginHeader]).to.equal('https://blah.example.com:234');
	});

	it('Returns no origin header if supplied origin does not match whitelist', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});

		expect(cors.getHeaders('http://microsoft.com')[accessControlAllowOriginHeader]).to.equal(undefined);
	});

	it('Returns allowed methods header only for preflight requests when origin matches', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});

		expect(cors.getHeaders('https://matches.example.com:234', undefined, 'OPTIONS')[accessControlAllowMethodsHeader]).to.equal(expectedAllowedMethods);
		expect(cors.getHeaders('http://does-not-match.com')[accessControlAllowMethodsHeader]).to.equal(undefined);
		expect(cors.getHeaders('https://matches.example.com:234')[accessControlAllowMethodsHeader]).to.equal(undefined);
	});

	it('Echoes back requested header when origin matches', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});
		var requestCorsHeaders = 'any, list-of, things-that-look-l1ke-headers';

		expect(cors.getHeaders('https://matches.example.com:234', requestCorsHeaders)[accessControlAllowHeadersHeader]).to.equal(requestCorsHeaders);
	});

	it('Does not return allowed CORS headers if origin does not match', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});
		var requestCorsHeaders = 'any, list-of, things-that-look-l1ke-headers';

		expect(cors.getHeaders('https://mismatching', requestCorsHeaders)[accessControlAllowHeadersHeader]).to.equal(undefined);
	});

	it('Does not echo back requested headers if they are malformed', function() {
		var cors = corsModule({
			hostnames: [{
				host: '*.example.com'
			}]
		});
		var malformedHeaderList1 = 'illegal-*-char',
			malformedHeaderList2 = 'too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long, too-long, much-too-long';

		expect(cors.getHeaders('https://match.example.com', malformedHeaderList1)[accessControlAllowHeadersHeader]).to.equal(undefined);
		expect(cors.getHeaders('https://match.example.com', malformedHeaderList2)[accessControlAllowHeadersHeader]).to.equal(undefined);
	});
});
