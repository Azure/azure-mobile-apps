// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    path = require('path');

describe('azure-mobile-apps.configuration.loader', function () {
    var loader = requireWithRefresh('../../src/configuration/loader');

    before(function () {
        require('../../src/logger').configure();
    });

    it('loads configuration from single file', function () {
        var configuration = loader.loadPath('./files/tables/table1');
        expect(configuration).to.deep.equal({
            table1: { authenticate: true }
        });
    });

    it('loads configuration recursively from directory and adds to target', function () {
        var configuration = loader.loadPath('./files/tables');
        expect(configuration).to.deep.equal({
            table1: { authenticate: true },
            table2: { authenticate: false },
            table3: { softDelete: true }
        });
    });

    it('loads files specified with .js extension', function () {
        var configuration = loader.loadPath('./files/tables/table1.js');
        expect(configuration).to.deep.equal({
            table1: { authenticate: true }
        });
    });

    it('loads relative to basePath', function () {
        var configuration = loader.loadPath('./configuration/files/tables/table1.js', path.resolve(__dirname, '..'));
        expect(configuration).to.deep.equal({
            table1: { authenticate: true }
        });
    });

    it('loads with .json extension', function () {
        var configuration = loader.loadPath('./files/jsontables/table1.json');
        expect(configuration).to.have.property('table1');
    });

    it('merges json properties', function () {
        var configuration = loader.loadPath('./files/jsontables/table1');
        expect(configuration).to.have.property('table1');
        var table = configuration.table1;
        expect(table).to.have.property('json', true);
        expect(table).to.have.property('authenticate', true);
        expect(table.func).to.have.property('json', true);
        expect(table.func.toString()).to.equal('function () { }');
    });

    it('defaults to json property value', function () {
        var configuration = loader.loadPath('./files/jsontables/conflictDefinition');
        expect(configuration).to.deep.equal({
            conflictDefinition: { source: '.json', deep: { object: { conflict: 2 } }}
        });
    });

    it('merges correctly when loading directories', function () {
        var configuration = loader.loadPath('./files/jsontables');
        expect(configuration.conflictDefinition).to.deep.equal({ source: '.json', deep: { object: { conflict: 2 } } });
        var table = configuration.table1;
        expect(table).to.have.property('json', true);
        expect(table).to.have.property('authenticate', true);
        expect(table.func).to.have.property('json', true);
        expect(table.func.toString()).to.equal('function () { }');
    });
    
    it("loads apis using helper function correctly", function () {
        var configuration = loader.loadPath('./files/api');
        expect(configuration.custom1.get).to.be.a('function');
    });

    it('does not throw when target path does not exist', function () {
        expect(loader.loadPath('this/path/does/not/exist')).to.deep.equal({});
    });
});

function requireWithRefresh(path) {
    var modulePath = require.resolve(path);
    delete require.cache[modulePath];
    return require(path);
}
