var express = require('express'),
    mobileApps = require('azure-mobile-apps'),
    telemetry = require('../../telemetry'),
    supertest = require('supertest-as-promised'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.functional.telemetry', function () {
    var app, mobileApp

    before(function () {
        app = express()
        mobileApp = mobileApps({ skipVersionCheck: true, debug: true })
        mobileApp.tables.import(__dirname + '/tables')
        mobileApp.api.import(__dirname + '/api')
        mobileApp.use(telemetry)
        app.use(mobileApp)
    })

    it("adds telemetry header for table requests", function () {
        return supertest(app)
            .get('/tables/respond')
            .expect('zumo-compatibility', require('../../package.json').version)
            .expect(200)
    })

    it("adds telemetry header for custom api requests", function () {
        return supertest(app)
            .get('/api/respond')
            .expect('zumo-compatibility', require('../../package.json').version)
            .expect(203)
    })
})
