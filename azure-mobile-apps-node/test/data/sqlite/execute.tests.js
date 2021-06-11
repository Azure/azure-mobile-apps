var sqlite3 = require('sqlite3'),
    execute = require('../../../src/data/sqlite/execute')(new sqlite3.Database(':memory:')),
    expect = require('chai').use(require('chai-as-promised')).expect;

describe('azure-mobile-apps.data.sqlite.execute', function () {
    it("executes basic query", function () {
        return execute({ sql: "select 1 as test" }).then(function (result) {
            expect(result).to.deep.equal([{ test: 1 }]);
        });
    });

    it("executes query with parameters", function () {
        return execute({ sql: "select @p1 as test", parameters: { p1: 'test' } }).then(function (result) {
            expect(result).to.deep.equal([{ test: 'test' }]);
        });
    });

    it("executes query with array of parameters", function () {
        return execute({ sql: "select @p1 as test", parameters: [{ name: 'p1', value: 'test' }] }).then(function (result) {
            expect(result).to.deep.equal([{ test: 'test' }]);
        });
    });
    
    it("executes noop", function () {
        return execute({ noop: true }).then(function (result) {
            expect(result).to.be.undefined;
        });
    });
    
    it("rejects on error", function () {
        return expect(execute({ sql: "select * from nonexistent" })).to.be.rejectedWith('no such table: nonexistent');
    });
});
