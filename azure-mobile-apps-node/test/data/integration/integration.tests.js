// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var queries = require('../../../src/query'),
    config = require('../../appFactory').configuration(),
    expect = require('chai').use(require('chai-subset')).use(require('chai-as-promised')).expect;

describe('azure-mobile-apps.data.integration', function () {
    var index = require('../../../src/data'),
        data = index(config),
        cleanUp = require('../' + config.data.provider + '/integration.cleanUp'),
        table = {
            name: 'integration',
            containerName: 'integration',
            columns: { string: 'string', number: 'number', bool: 'boolean' }
        },
        operations;

    before(function (done) {
        operations = data(table);
        operations.initialize().then(done, done);
    });

    afterEach(function (done) {
        cleanUp(config.data, table).then(function (arg) { done() }, done);
    });

    it("basic integration test", function () {
        return insert({ id: '1', string: 'test', bool: false, number: 1.1 })
            .then(function (results) {
                expect(results).to.containSubset({ id: '1', string: 'test', bool: false, number: 1.1 });
                return read();
            })
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test', bool: false, number: 1.1 }]);
                return update({ id: '1', string: 'test2', bool: true, number: 2.2  });
            })
            .then(read)
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test2', bool: true, number: 2.2 }]);
                return del(1);
            })
            .then(read)
            .then(function (results) {
                expect(results.length).to.equal(0);
            });
    });

    it("preserves existing values if unspecified", function () {
        return insert({ id: '1', string: 'test', bool: true, number: 1.1 })
            .then(function () {
                return update({ id: '1', string: 'test2' });
            })
            .then(read)
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test2', bool: true, number: 1.1 }]);
                return del(1);
            });
    });

    it("returns deleted record", function () {
        var item = { id: '1', string: 'test', bool: true, number: 1.1 };
        return insert(item)
            .then(function () {
                return del('1');
            })
            .then(function (results) {
                expect(results).to.containSubset(item);
                return expect(del('1')).to.be.rejectedWith('No records were updated');
            });
    });

    it("returns softDeleted record", function () {
        operations = data({
            name: 'integration',
            containerName: 'integration',
            softDelete: true,
            columns: { string: 'string', number: 'number', bool: 'boolean' }
        });

        var item = { id: '1', string: 'test', bool: true, number: 1.1 };
        return insert(item)
            .then(function () {
                return del('1');
            })
            .then(function (results) {
                expect(results).to.containSubset(item);
                return expect(del('1')).to.be.rejectedWith('No records were updated');
            });
    });

    it("handles large numeric values", function () {
        return insert({ id: '1', string: 'test', number: null })
            .then(read)
            .then(function (results) {
                expect(results[0].number).to.equal(null);
                expect(results[0].string).to.equal('test');
                return update({ id: '1', string: null, number: 1 })
            })
            .then(read)
            .then(function (results) {
                expect(results[0].number).to.equal(1);
                expect(results[0].string).to.equal(null);
            });
    });

    it("handles null values", function () {
        return insert({ id: '1', number: Number.MAX_VALUE })
            .then(read)
            .then(function (results) {
                expect(results[0].number).to.equal(Number.MAX_VALUE);
            });
    });

    it("handles zero values", function () {
        return insert({ id: '1', number: 0 })
            .then(read)
            .then(function (results) {
                expect(results[0].number).to.equal(0);
            });
    });

    it("handles read operations with no query", function () {
        return insert({ id: '1' })
            .then(function () {
                return operations.read();
            })
            .then(function (results) {
                expect(results.length).to.equal(1);
            });
    });

    it("handles updates with queries", function () {
        return insert({ id: '1', p1: 1 })
            .then(function () {
                return operations.update({ id: '1', p1: 2 }, queries.create('integration').where({ p1: 1 }));
            })
            .then(function (result) {
                expect(result).to.containSubset({ id: '1', p1: 2 });
                return operations.update({ id: '1', p1: 3 }, queries.create('integration').where({ p1: 1 }));
            })
            .then(function () {
                throw new Error('Update succeeded when it should have failed');
            })
            .catch(function () { });
    });

    it("handles deletes with queries", function () {
        return insert({ id: '1', p1: 1 })
            .then(function () {
                return operations.delete(queries.create('integration').where({ id: 1, p1: 2 }));
            })
            .then(function () {
                throw new Error('Update succeeded when it should have failed');
            })
            .catch(function () {
                return operations.delete(queries.create('integration').where({ id: 1, p1: 1 }));
            });
    });

    it("handles deletes of multiple records", function () {
        return insert({ id: '1', p1: 1 })
            .then(function () {
                return insert({ id: '2', p1: 1 })
            })
            .then(function () {
                return insert({ id: '3', p1: 2 })
            })
            .then(function () {
                return operations.delete(queries.create('integration').where({ p1: 1 }));
            })
            .then(function (deleted) {
                expect(deleted.length).to.equal(2);
            });
    });

    it("handles multiple schema updates", function () {
        return insert({ id: '1', p1: 1.1 })
            .then(function () {
                return insert({ id: '2', p1: 2.2, p2: 'test2', p3: false })
            })
            .then(read)
            .then(function (results) {
                expect(results).to.containSubset([
                    { id: '1', p1: 1.1, p2: null, p3: null },
                    { id: '2', p1: 2.2, p2: 'test2', p3: false }
                ]);
            });
    });

    it("ignores null columns on table creation", function () {
        return insert({ id: '1', p1: null, p2: '1' })
            .then(read)
            .then(function (results) {
                expect(results.length).to.equal(1);
                expect(results[0].p1).to.not.be.ok;
                expect(results[0].p2).to.equal('1');
            });
    });

    it("ignores null columns on schema update", function () {
        return insert({ id: '1' })
            .then(function () {
                return insert({ id: '2', p1: null, p2: '1' });                
            })
            .then(read)
            .then(function (results) {
                expect(results.length).to.equal(2);
                expect(results[1].p1).to.not.be.ok;
                expect(results[1].p2).to.equal('1');
            });
    });

    it("handles object based queries", function () {
        return insert({ id: '1', string: 'test', bool: false, number: 1.1 })
            .then(function (results) {
                expect(results).to.containSubset({ id: '1', string: 'test', bool: false, number: 1.1 });
                return operations.read({ id: '1' });
            })
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test', bool: false, number: 1.1 }]);
                return operations.update({ id: '1', string: 'test2', bool: true, number: 2.2  }, { id: '1' });
            })
            .then(read)
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test2', bool: true, number: 2.2 }]);
                return operations.delete({ id: '1' });
            })
            .then(read)
            .then(function (results) {
                expect(results.length).to.equal(0);
            });
    });

    it("handles predicate functions", function () {
        return insert({ id: '1', string: 'test', bool: false, number: 1.1 })
            .then(function (results) {
                expect(results).to.containSubset({ id: '1', string: 'test', bool: false, number: 1.1 });
                return operations.read(predicate);
            })
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test', bool: false, number: 1.1 }]);
                return operations.update({ id: '1', string: 'test2', bool: true, number: 2.2  }, predicate);
            })
            .then(read)
            .then(function (results) {
                expect(results).to.containSubset([{ id: '1', string: 'test2', bool: true, number: 2.2 }]);
                return operations.delete(predicate);
            })
            .then(read)
            .then(function (results) {
                expect(results.length).to.equal(0);
            });
        
        function predicate() {
            return this.id === '1';
        }
    });

    function read() {
        return operations.read(queries.create('integration'));
    }

    function insert(item) {
        return operations.insert(item);
    }

    function update(item) {
        return operations.update(item);
    }

    function del(id) {
        return operations.delete(queries.create('integration').where({ id: id }));
    }
});
