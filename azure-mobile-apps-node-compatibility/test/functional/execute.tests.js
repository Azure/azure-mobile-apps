var express = require('express'),
    mobileApps = require('azure-mobile-apps'),
    supertest = require('supertest-as-promised'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.functional.execute', function () {
    var app, mobileApp

    before(function () {
        app = express()
        mobileApp = mobileApps({ skipVersionCheck: true, debug: true })
        mobileApp.tables.import(__dirname + '/tables')
        app.use(mobileApp)
    })

    it("executes statements", function () {
        return supertest(app)
            .post('/tables/execute')
            .send({ id: "1", value: "test" })
            .expect(201)
            .then(function () {
                return supertest(app)
                    .patch('/tables/execute/1')
                    .send({ value: "test2" })
                    .expect(200)
            })
            .then(function () {
                return supertest(app)
                    .get('/tables/execute/1')
                    .expect(200)
            })
            .then(function (item) {
                expect(item.body).to.containSubset({ id: "1", value: "test2" })
                return supertest(app)
                    .delete('/tables/execute/1')
                    .expect(200)
            })
    })
})
