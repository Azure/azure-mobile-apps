var serializeModule = require('../../../src/data/sqlite/serialize'),
    promises = require('../../../src/utilities/promises'),
    sqlite3 = require('sqlite3'),
    expect = require('chai').use(require('chai-as-promised')).expect;

describe('azure-mobile-apps.data.sqlite.serialize', function () {
    it("executes multiple transactions in order", function () {
        var serialize = serializeModule(new sqlite3.Database(':memory:')),
            firstExecuted = false;
        
        serialize([
            { sql: "create table test (id integer primary key)" },
            { sql: "insert into test default values" },
            { sql: "insert into test default values" },
            { sql: "select * from test" }
        ]).then(function (rows) {
            expect(rows).to.deep.equal([{ id: 1 }, { id: 2 }]);
            firstExecuted = true;
        });
        
        return serialize([
            { sql: 'insert into test default values' },
            { sql: "select * from test" }
        ]).then(function (rows) {
            expect(firstExecuted).to.be.true;
            expect(rows).to.deep.equal([{ id: 1 }, { id: 2 }, { id: 3 }]);            
        });
    });
    
    it("isolates multiple transactions", function () {
        var serialize = serializeModule(new sqlite3.Database(':memory:'));
        return serialize([{ sql: 'create table test (value text)' }])
            .then(function () {
                return promises.all([
                    serialize(createStatements('1')).then(verifyResult('1')),
                    serialize(createStatements('2')).then(verifyResult('2')),
                    serialize(createStatements('3')).then(verifyResult('3'))
                ]);
            });
            
        function createStatements(value) {
            return [
                { sql: 'delete from test' },
                { sql: 'insert into test values (@value)', parameters: { value: value } },
                { sql: 'select * from test' }
            ];
        }
        
        function verifyResult(value) {
            return function (result) {
                expect(result).to.deep.equal([{ value: value }]);
            };
        }
    });

    it("isolates generated primary keys", function () {
        var serialize = serializeModule(new sqlite3.Database(':memory:'));
        return serialize([{ sql: 'create table test (id integer primary key)' }])
            .then(function () {
                return promises.all([
                    serialize(createStatements()).then(verifyResult(1)),
                    serialize(createStatements()).then(verifyResult(2)),
                    serialize(createStatements()).then(verifyResult(3))
                ]);
            });
            
        function createStatements() {
            return [
                { sql: 'insert into test default values' },
                { sql: 'select last_insert_rowid() as rowid' }
            ];
        }
        
        function verifyResult(value) {
            return function (result) {
                expect(result).to.deep.equal([{ rowid: value }]);
            };
        }
    });
});