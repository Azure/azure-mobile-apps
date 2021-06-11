// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var fromCommandLine = require('../../src/configuration/from/commandLine'),
    expect = require('chai').use(require('chai-subset')).expect,
    q = require('q');

describe('azure-mobile-apps.configuration.from.commandLine', function () {
    it('applies settings from supplied arguments', function () {
        var args = ['---logging.level', 'silly', '---promiseConstructor', 'q'];
        expect(fromCommandLine({}, args)).to.containSubset({
            logging: { level: 'silly' },
            promiseConstructor: q.Promise
        })
    });
});
