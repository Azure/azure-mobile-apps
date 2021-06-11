// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var normalizeClaims = require('../../src/auth/normalizeClaims'),
    expect = require('chai').expect;

describe('azure-mobile-apps.auth.normalizeClaims', function () {
	var responseFromServer = [
		{
			'provider_name': 'p1',
			'access_token': 'this-is-an-access-token',
			'user_id': 'username',
			'user_claims': [
				{ 'typ': 'urn:p1:name', val: 'My Name' },
				{ 'typ': 'urn:p1:id', val: 'My ID' }
			]
		},
		{
			'provider_name': 'p2',
			'access_token': 'p2-access-token',
			'user_id': 'p2-name',
			'user_claims': [
				{ 'typ': 'urn:p2:id', val: 'My P2 ID' },
				{ 'typ': 'http://schemas.xmlsoap.org/ws/name', val: 'Name Field' }
			]
		}
	];

	it('converts the provider array to an object', function () {
		var n = normalizeClaims(responseFromServer);

		expect(n.p1).to.be.an('object');
		expect(n.p1).to.have.property('provider_name');
		expect(n.p1).to.have.property('access_token');
		expect(n.p1).to.have.property('user_id');
		expect(n.p1).to.have.property('user_claims');

		expect(n.p2).to.be.an('object');
		expect(n.p2).to.have.property('provider_name');
		expect(n.p2).to.have.property('access_token');
		expect(n.p2).to.have.property('user_id');
		expect(n.p2).to.have.property('user_claims');
	});

	it('converts claims to an object', function () {
		var n = normalizeClaims(responseFromServer);

		expect(n.p1).to.have.property('claims');
		expect(n.p1.claims).to.be.an('object');
		expect(n.p1.claims).to.have.property('urn:p1:name', 'My Name');
		expect(n.p1.claims).to.have.property('name', 'My Name');
		expect(n.p1.claims).to.have.property('urn:p1:id', 'My ID');
		expect(n.p1.claims).to.have.property('id', 'My ID');

		expect(n.p2).to.have.property('claims');
		expect(n.p2.claims).to.be.an('object');
		expect(n.p2.claims).to.have.property('urn:p2:id', 'My P2 ID');
		expect(n.p2.claims).to.have.property('id', 'My P2 ID');
		expect(n.p2.claims).to.have.property('http://schemas.xmlsoap.org/ws/name', 'Name Field');
		expect(n.p2.claims).to.have.property('name', 'Name Field');
	});

	it('copies the access token', function () {
		var n = normalizeClaims(responseFromServer);
		expect(n.p1.access_token).to.equal('this-is-an-access-token');
		expect(n.p2.access_token).to.equal('p2-access-token');
	});

	it('copies the user id', function () {
		var n = normalizeClaims(responseFromServer);
		expect(n.p1.user_id).to.equal('username');
		expect(n.p2.user_id).to.equal('p2-name');
	});

    it('handles single objects', function () {
        var n = normalizeClaims({
			'provider_name': 'p1',
			'access_token': 'this-is-an-access-token',
			'user_id': 'username',
			'user_claims': [
				{ 'typ': 'urn:p1:name', val: 'My Name' },
				{ 'typ': 'urn:p1:id', val: 'My ID' }
			]
        });

        expect(n.p1).to.be.an('object');
		expect(n.p1).to.have.property('provider_name');
		expect(n.p1).to.have.property('access_token');
		expect(n.p1).to.have.property('user_id');
		expect(n.p1).to.have.property('user_claims');
    });

    it('adds groups to an array', function () {
        var n = normalizeClaims({
			'provider_name': 'p1',
			'user_claims': [ { 'typ': 'groups', val: 'group' } ]
        });

        expect(n.p1.claims.groups).to.be.an('array');
        expect(n.p1.claims.groups.length).to.equal(1);
    });

    it('converts claim types with multiple values to an array', function () {
        var n = normalizeClaims({
			'provider_name': 'p1',
			'user_claims': [
                { 'typ': 'claimType', val: 'claim1' } ,
                { 'typ': 'claimType', val: 'claim2' }
            ]
        });

        expect(n.p1.claims.claimType).to.be.an('array');
        expect(n.p1.claims.claimType.length).to.equal(2);
    })
});
