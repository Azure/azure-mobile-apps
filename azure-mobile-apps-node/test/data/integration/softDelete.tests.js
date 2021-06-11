// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var config = require('../../appFactory').configuration().data,
    queries = require('../../../src/query'),
    expect = require('chai').expect;

describe('azure-mobile-apps.data.integration.softDelete', function () {
    var index = require('../../../src/data/' + config.provider),
        data = index(config),
        cleanUp = require('../' + config.provider + '/integration.cleanUp'),
        table = { name: 'softDelete', softDelete: true },
        operations;

    before(function () {
        operations = data(table);
    });

    afterEach(function (done) {
        cleanUp(config, table).then(done, done);
    });

    it('deleted records are not returned with normal query', function () {
        return insert({ id: '1' })
            .then(function () {
                return del('1');
            })
            .then(function () {
                return read();
            })
            .then(function(results) {
                expect(results.constructor).to.equal(Array);
                expect(results.length).to.equal(0);
            });
    });

    it('deleted records are returned when requested', function () {
        return insert({ id: '1' })
            .then(function () {
                return del('1');
            })
            .then(function () {
                return read(true);
            })
            .then(function(results) {
                expect(results.constructor).to.equal(Array);
                expect(results.length).to.equal(1);
            });
    });

    it('deleted records can be undeleted', function () {
        return insert({ id: '1' })
            .then(function () {
                return del('1');
            })
            .then(function () {
                return undelete('1');
            })
            .then(function () {
                return read();
            })
            .then(function(results) {
                expect(results.constructor).to.equal(Array);
                expect(results.length).to.equal(1);
            });
    });

    function read(includeDeleted) {
        var query = queries.create('softDelete');
        if(includeDeleted) query.includeDeleted();
        return operations.read(query);
    }

    function insert(item) {
        return operations.insert(item);
    }

    function update(item) {
        return operations.update(item);
    }

    function del(id, version) {
        var query = queries.create('integration').where({ id: id });
        return operations.delete(query, version);
    }

    function undelete(id, version) {
        var query = queries.create('integration').where({ id: id });
        return operations.undelete(query, version);
    }
});
