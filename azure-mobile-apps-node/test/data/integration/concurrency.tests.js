// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var config = require('../../appFactory').configuration().data,
    queries = require('../../../src/query'),
    promises = require('../../../src/utilities/promises'),
    expect = require('chai').expect;

describe('azure-mobile-apps.data.integration.concurrency', function () {
    var index = require('../../../src/data/' + config.provider),
        data = index(config),
        cleanUp = require('../' + config.provider + '/integration.cleanUp'),
        table = { name: 'concurrency' },
        operations;

    before(function (done) {
        operations = data(table);
        operations.initialize().then(done, done);
    });

    beforeEach(function (done) {
        operations.truncate().then(done, done);
    });

    after(function (done) {
        cleanUp(config, table).then(done, done);
    });

    it('assigns value to version column', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                expect(inserted.version).to.not.be.undefined;
            });
    });

    it('does not update items with incorrect version', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                return update({ id: '1', value: 'test2', version: 'no match' });
            })
            .then(function () {
                throw new Error('Record with mismatching version was updated');
            }, function (error) {
                expect(error.concurrency).to.be.true;
                expect(error.item).to.not.be.undefined;
            });
    });

    it('updates items with correct version', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                return update({ id: '1', value: 'test2', version: inserted.version });
            })
            .then(function () { }, function () {
                throw new Error('Record with matching version was not updated');
            });
    });

    it('updates version with greater value on update', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                return update({ id: '1', value: 'test2', version: inserted.version })
                    .then(function (updated) {
                        expect(decodeBase64(updated.version)).to.be.greaterThan(decodeBase64(inserted.version));
                    });
            });
    });

    it('does not delete items with incorrect version', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                return del('1', 'no match');
            })
            .then(function () {
                throw new Error('Record with mismatching version was deleted');
            }, function (error) {
                expect(error.concurrency).to.be.true;
                expect(error.item).to.not.be.undefined;
            });
    });

    it('deletes items with correct version', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                return del('1', inserted.version);
            })
            .then(function () { }, function (error) {
                throw new Error('Record with matching version was not deleted: ' + error.message);
            });
    });

    it('throws duplicate error when inserting rows with duplicate IDs', function () {
        return insert({ id: '1', value: 'test' })
            .then(function (inserted) {
                return insert({ id: '1', value: 'test' })
            })
            .then(function () {
                throw new Error('Succeeded inserting duplicate ID');
            }, function (error) {
                expect(error.duplicate).to.be.true;
            });
    });

    it('executes multiple statements concurrently', function () {
        var dynamic = data({ name: 'concurrency', dynamicSchema: true }),
            static = data({ name: 'concurrency', dynamicSchema: false }),
            dynamicComplete, staticComplete;

        return promises.all([
            dynamic.insert({ id: '1' }),
            static.insert({ id: '2' })
        ]);
    })

    function read() {
        return operations.read(queries.create('integration'));
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

    function decodeBase64(value) {
        return new Buffer(value, 'base64').toString("ascii");
    }
});
