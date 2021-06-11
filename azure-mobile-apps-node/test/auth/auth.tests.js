// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var authModule = require('../../src/auth'),
    expect = require('chai').expect;

describe('azure-mobile-apps.auth', function () {
    it('signs and validates tokens', function () {
        var auth = authModule({ secret: 'secret' }),
            token = auth.sign({ claim: 'claim', sub: 'id' });

        return auth.validate(token).then(function (user) {
            expect(user.id).to.equal('id');
        });
    });

    it('validates audience and issuer', function () {
        var auth = authModule({ secret: 'secret' }),
            audienceChecker = authModule({ secret: 'secret', audience: 'audience' }),
            issuerChecker = authModule({ secret: 'secret', issuer: 'issuer' }),
            token = auth.sign({ claim: 'claim', sub: 'id' });

        return audienceChecker.validate(token).then(expect.fail)
            .catch(function () {
                return issuerChecker.validate(token).then(expect.fail).catch(function () {});
            });
    });

    it('payload expiry, issuer and audience takes precedence over options', function () {
        var auth = authModule({ secret: 'secret', expires: 1440, audience: 'configAudience', issuer: 'configIssuer' }),
            token = auth.sign({ claim: 'claim', sub: 'id', exp: 9999999, aud: 'payloadAudience', iss: 'payloadIssuer' }),
            decodedClaims = auth.decode(token).claims;

        expect(decodedClaims.exp).to.equal(9999999);
        expect(decodedClaims.aud).to.equal('payloadAudience');
        expect(decodedClaims.iss).to.equal('payloadIssuer');
    });

    it('handles hex encoded secrets', function () {
        var auth = authModule({ secret: 'abc' }),
            token = auth.sign({ sub: 'testUser' });
        auth = authModule({ azureSigningKey: '616263' });
        return auth.validate(token)
            .then(function (validated) {
                expect(validated.id).to.equal('testUser');

                auth = authModule({ azureSigningKey: '616263' });
                token = auth.sign({ sub: 'testUser' });
                auth = authModule({ secret: 'abc' });
                return auth.validate(token);
            })
            .then(function (validated) {
                expect(validated.id).to.equal('testUser');
            });
    });
});
