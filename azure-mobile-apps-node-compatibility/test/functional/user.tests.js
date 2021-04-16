var express = require('express'),
    mobileApps = require('azure-mobile-apps'),
    auth = require('azure-mobile-apps/src/auth')({ secret: 'secret' }),
    supertest = require('supertest-as-promised'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.functional.user', function () {
    var app, mobileApp

    before(function () {
        app = express()
        mobileApp = mobileApps({ skipVersionCheck: true, debug: true, configFile: __dirname + '/azureMobile.js', auth: { secret: 'secret' } })
        mobileApp.tables.import(__dirname + '/tables')
        mobileApp.api.import(__dirname + '/api')
        app.use(mobileApp)
    })

    it("exposes anonymous user when not authenticated", function () {
        supertest(app)
            .get('/api/user')
            .expect(200)
            .then(function (results) {
                expect(results.body.level).to.equal('anonymous')
                expect(results.body).to.not.have.property('userId')
            })
    })

    it("exposes authenticated user when authenticated", function () {
        supertest(app)
            .get('/api/user')
            .set('x-zumo-auth', auth.sign({ sub: 'testuser' }))
            .expect(200)
            .then(function (results) {
                expect(results.body.level).to.equal('authenticated')
                expect(results.body.userId).to.equal('testuser')
            })
    })
})
