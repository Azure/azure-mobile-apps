// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var express = require('express'),
    bodyParser = require('body-parser'),
    request = require('../../src/utilities/request'),
    expect = require('chai').expect;

describe('azure-mobile-apps.utilities.request', function () {
    it("POSTs to specified endpoint with specified payload", function (done) {
        var app = express();
        app.post('/test', bodyParser.json(), function (req) {
            expect(req.body).to.deep.equal({ value: 'test' });
            done();
        });
        var listener = app.listen(0);
        request('http://localhost:' + listener.address().port + '/test', { value: 'test' });
    });
});
