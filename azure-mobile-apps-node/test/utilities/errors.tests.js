// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
﻿var expect = require('chai').expect,
    errors = require('../../src/utilities/errors');

describe('azure-mobile-apps.utilities.errors', function () {
    it('creates error objects', function () {
        expect(errors.concurrency()).to.be.an.instanceof(Error);
    });

    it('formats messages', function () {
        expect(errors.badRequest('te%s', 'st').message).to.equal('test');
    });
});
