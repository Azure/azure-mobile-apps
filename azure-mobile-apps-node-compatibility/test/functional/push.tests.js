var express = require('express'),
    mobileApps = require('azure-mobile-apps'),
    supertest = require('supertest-as-promised'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.functional.push', function () {
    var app, mobileApp

    before(function () {
        app = express()
        mobileApp = mobileApps({ skipVersionCheck: true, debug: true, notifications: { hubName: 'test', connectionString: '<connectionString>' } })
        mobileApp.tables.import(__dirname + '/tables')
        mobileApp.api.import(__dirname + '/api')
        app.use(mobileApp)
    })

    it("populates push pseudo global if configured", function () {
        return supertest(app)
            .get('/tables/push')
            .expect(200)
            .then(function (response) {
                expect(response.body.sendExists).to.be.true
            })
    })
})
