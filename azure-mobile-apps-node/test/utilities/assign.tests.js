// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    assign = require('../../src/utilities/assign');

// We've had a few issues with various implementations of a deep assign algorithm. These are the behaviors we need.
describe('azure-mobile-apps.utilities.assign', function () {
    it('merges where property already exists on target', function () {
        var child = {},
            result = assign({ a: {} }, { a: child });
        expect(result.a).to.not.equal(child);
    });

    it('merges where property does not exist on target', function () {
        var child = {},
            result = assign({ }, { a: child });
        expect(result.a).to.not.equal(child);
    });

    it('assigns function to string target property', function () {
        var child = function () {},
            result = assign({ a: '' }, { a: child });
        expect(result.a).to.equal(child);
    });

    it('assigns function to object target property', function () {
        var child = function () {},
            result = assign({ a: {} }, { a: child });
        expect(result.a).to.equal(child);
    });
});
