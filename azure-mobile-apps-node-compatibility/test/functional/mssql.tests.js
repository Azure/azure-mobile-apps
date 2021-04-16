var express = require('express'),
    mobileApps = require('azure-mobile-apps'),
    supertest = require('supertest-as-promised'),
    expect = require('chai').use(require('chai-subset')).expect

if(mobileApps().configuration.data.provider === 'mssql') {
    describe('azure-mobile-apps-compatibility.functional.mssql', function () {
        var app, mobileApp

        before(function () {
            app = express()
            mobileApp = mobileApps({ skipVersionCheck: true, debug: true, configFile: __dirname + '/azureMobile.js' })
            mobileApp.tables.import(__dirname + '/tables')
            mobileApp.api.import(__dirname + '/api')
            app.use(mobileApp)
        })

        it("executes parameterised statements (requires configured database)", function () {
            return supertest(app)
                .get('/api/mssql')
                .expect(200)
                .then(function (results) {
                    expect(results.body).to.deep.equal([{ value: 1 }])
                })
        })
    })
}