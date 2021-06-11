// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').use(require('chai-subset')).expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/sqlite/statements'),
    queries = require('../../../../src/query');

describe('azure-mobile-apps.data.sqlite.statements', function () {
    describe('delete', function () {
        var del = statements.delete;

        it('generates simple statement and parameters', function () {
            var statement = del({ name: 'table' }, queries.create('table').where({ id: 'id' }));
            expect(statement).to.containSubset([
                { sql: 'SELECT * FROM [table] WHERE ([id] = @p1)', parameters: { p1: 'id' } },
                { sql: 'DELETE FROM [table] WHERE ([id] = @p1)', parameters: { p1: 'id' } },
                { sql: 'SELECT changes() AS recordsAffected' },
            ]);
        });

        it('generates soft delete statement and params', function () {
            var statement = del({ name: 'table', softDelete: true }, queries.create('table').where({ id: 'id' }));
            expect(statement).to.containSubset([
                { sql: 'UPDATE [table] SET [deleted] = 1 WHERE ([id] = @p1) AND [deleted] = 0', parameters: { p1: 'id' } },
                { sql: 'SELECT * FROM [table] WHERE ([id] = @p1)', parameters: { p1: 'id' } },
                { sql: 'SELECT changes() AS recordsAffected' },
            ]);
        });
    });
});
