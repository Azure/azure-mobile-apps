var app = require('express')(),
    mobileApp = require('azure-mobile-apps')({ homePage: true }),
    telemetry = require('azure-mobile-apps-compatibility/telemetry')

mobileApp.tables.import('tables')
mobileApp.api.import('api')
mobileApp.use(telemetry)
app.use(mobileApp)
app.listen(process.env.PORT || 3000)
