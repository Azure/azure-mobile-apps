var configuration = require('./appFactory').configuration(),
    configureGlobals = require('../src/configuration/from').configureGlobals;

beforeEach(function () {
    configureGlobals(configuration);
});
