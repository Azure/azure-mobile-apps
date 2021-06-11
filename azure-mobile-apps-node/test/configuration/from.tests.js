// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var from = require('../../src/configuration/from'),
    expect = require('chai').use(require('chai-subset')).expect;

describe('azure-mobile-apps.configuration.from', function () {
    it("exposes fluent API", function () {
        expect(from()
            .defaults()
            .commandLine(['---logging.level', 'silly'])
            .environment({ 'website_site_name': 'testName' })
            .apply()
        ).to.containSubset({
            platform: 'express',
            logging: { level: 'silly' },
            name: 'testName'
        });
    });
});
