// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    queries = require('../../../../src/query'),
    statements = require('../../../../src/data/mssql/statements');

describe('azure-mobile-apps.data.mssql.statements', function () {
    describe('update', function () {
        var update = statements.update;

        it('generates simple statement and parameters', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value', p2: 2.2 });
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [p1] = @p1,[p2] = @p2 WHERE [id] = @id; SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM [dbo].[table] WHERE [id] = @id');
            expect(statement.parameters).to.deep.equal([{ name: 'p1', type: mssql.NVarChar(), value: 'value' }, { name: 'p2', type: mssql.Float, value: 2.2 }, { name: 'id', type: mssql.NVarChar(255), value: 'id' }]);
        });

        it('updates null values correctly', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: null });
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [p1] = @p1 WHERE [id] = @id; SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM [dbo].[table] WHERE [id] = @id');
            expect(statement.parameters).to.deep.equal([{ name: 'p1', type: undefined, value: null }, { name: 'id', type: mssql.NVarChar(255), value: 'id' }]);
        });

        it('updates zero values correctly', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 0 });
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [p1] = @p1 WHERE [id] = @id; SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM [dbo].[table] WHERE [id] = @id');
            expect(statement.parameters).to.deep.equal([{ name: 'p1', type: mssql.Int, value: 0 }, { name: 'id', type: mssql.NVarChar(255), value: 'id' }]);
        });

        it('does not throw if item contains version', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value', p2: 2.2, version: '1' });
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [p1] = @p1,[p2] = @p2 WHERE [id] = @id AND [version] = @version; SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM [dbo].[table] WHERE [id] = @id');
        });

        it('adds queries to where clause', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value' }, queries.create('table').where({ p2: 2 }));
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [p1] = @p1 WHERE [id] = @id AND ([p2] = @q1); SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM [dbo].[table] WHERE [id] = @id AND ([p2] = @q1)');
            expect(statement.parameters).to.deep.equal([
                { name: 'p1', type: mssql.NVarChar(), value: 'value' },
                { name: 'id', type: mssql.NVarChar(255), value: 'id' },
                // queryjs doesn't generate parameters how we would ideally like...
                { name: 'q1', type: undefined, value: 2, pos: 1 }
            ]);
        });

        it('does not add empty queries to where clause', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value' }, queries.create('table'));
            expect(statement.sql).to.equal('UPDATE [dbo].[table] SET [p1] = @p1 WHERE [id] = @id; SELECT @@ROWCOUNT as recordsAffected; SELECT * FROM [dbo].[table] WHERE [id] = @id');
            expect(statement.parameters).to.deep.equal([
                { name: 'p1', type: mssql.NVarChar(), value: 'value' },
                { name: 'id', type: mssql.NVarChar(255), value: 'id' },
            ]);
        })
    });
});
