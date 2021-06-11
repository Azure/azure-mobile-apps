// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var fromDefaults = require('../../src/configuration/from/defaults'),
    expect = require('chai').expect;

describe('azure-mobile-apps.configuration.from.defaults', function () {
    it('returns defaults with no arguments', function () {
        expect(fromDefaults().platform).to.equal('express');
    });

    it('supplied configuration overrides defaults', function () {
        expect(fromDefaults({}, { platform: 'test' }).platform).to.equal('test');
    });
});
