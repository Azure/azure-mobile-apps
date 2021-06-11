// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').use(require('chai-subset')).expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/mssql/statements'),
    queries = require('../../../../src/query');

describe('azure-mobile-apps.data.mssql.statements', function () {
    describe('delete', function () {
        var del = statements.delete;

        it('generates simple statement and parameters', function () {
            var statement = del({ name: 'table' }, queries.create('table').where({ id: 'id' }));
            expect(statement.sql).to.equal('SELECT * FROM [dbo].[table] WHERE ([id] = @p1);DELETE FROM [dbo].[table] WHERE ([id] = @p1);SELECT @@rowcount AS recordsAffected;');
            expect(statement.parameters).to.containSubset([{ name: 'p1', value: 'id' }]);
        });

        it('generates soft delete statement and params', function () {
            var statement = del({ name: 'table', softDelete: true }, queries.create('table').where({ id: 'id' }));
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [deleted] = 1 WHERE ([id] = @p1) AND [deleted] = 0;SELECT @@rowcount AS recordsAffected;SELECT * FROM [dbo].[table] WHERE ([id] = @p1);');
            expect(statement.parameters).to.containSubset([{ name: 'p1', value: 'id' }]);
        });
    });
});
