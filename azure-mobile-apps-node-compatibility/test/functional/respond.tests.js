var express = require('express'),
    mobileApps = require('azure-mobile-apps'),
    supertest = require('supertest-as-promised')

describe('azure-mobile-apps-compatibility.functional.respond', function () {
    var app, mobileApp

    before(function () {
        app = express()
        mobileApp = mobileApps({ skipVersionCheck: true, debug: true })
        mobileApp.tables.import(__dirname + '/tables')
        mobileApp.api.import(__dirname + '/api')
        app.use(mobileApp)
    })

    describe('table', function () {
        it("responds with default response", function () {
            return supertest(app)
                .get('/tables/respond')
                .expect(200)
        })

        it("responds with error", function () {
            return supertest(app)
                .post('/tables/respond')
                .expect(500)
        })

        it("responds with specified response", function () {
            return supertest(app)
                .patch('/tables/respond')
                .expect(503)
        })
    })

    describe('api', function () {
        it("responds with request.respond", function () {
            return supertest(app)
                .get('/api/respond')
                .expect(203)
        })

        it("responds with res.send", function () {
            return supertest(app)
                .post('/api/respond')
                .expect(202)
        })
    })
})
