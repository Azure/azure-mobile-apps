// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var config = require('../../appFactory').configuration().data,
    queries = require('../../../src/query'),
    expect = require('chai').use(require('chai-subset')).use(require('chai-as-promised')).expect;

describe('azure-mobile-apps.data.integration.schema', function () {
    var index = require('../../../src/data/' + config.provider),
        data = index(config),
        cleanUp = require('../' + config.provider + '/integration.cleanUp'),
        table = { name: 'schemaTest', seed: [{ id: '1' }, { id: '2' }] },
        operations = data(table);

    afterEach(function (done) {
        cleanUp(config, table).then(done, done);
    });

    it("initialize creates table and seeds", function () {
        return operations.initialize(table)
            .then(function () {
                return read();
            })
            .then(function (results) {
                expect(results.length).to.equal(2);
            })
    });

    it("initialize handles existing tables", function () {
        return operations.initialize(table)
            // .then(function () {
            //     return operations.initialize(table);
            // })
            .then(function () {
                return read();
            })
            .then(function (results) {
                expect(results.length).to.equal(2);
            })
    });

    it("schema returns database schema for table", function () {
        return operations.initialize(table)
            .then(function () {
                return operations.schema(table);
            })
            .then(function (tableSchema) {
                expect(tableSchema).to.containSubset({
                    name: table.name,
                    properties: [
                        { name: 'id', type: 'string' }
                    ]
                });
            });
    });

    function read() {
        return operations.read(queries.create('schemaTest'));
    }
});
