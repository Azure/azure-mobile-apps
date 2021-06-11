var transactions = require('../../../src/data/sqlite/transactions'),
    promises = require('../../../src/utilities/promises'),
    sqlite3 = require('sqlite3'),
    expect = require('chai').use(require('chai-as-promised')).expect;

describe('azure-mobile-apps.data.sqlite.transactions', function () {
    var statements = [
        { sql: "create table test (id integer primary key)" },
        { sql: "insert into test default values" },
        { sql: "insert into test default values" },
        { sql: "select * from test" }
    ];
    
    it("executes multiple statements", function () {
        return transactions(new sqlite3.Database(':memory:'))(statements).then(function (rows) {
            expect(rows).to.deep.equal([{ id: 1 }, { id: 2 }]);
        });
    });
});