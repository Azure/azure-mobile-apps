// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    templates = require('../../src/templates');

describe('azure-mobile-apps.templates', function () {
    it("returns specified file name", function () {
        expect(templates('authStub.html')).to.contain('authentication development stub');
    });

    // keep coming up against this and end up not needing it
    // it("renders specified template through util.format", function () {
    //     expect(templates('authStub.html', '1', '2', '3')).to.contain('ID: 3');
    // });
});
