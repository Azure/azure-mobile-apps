// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/mssql/statements');

describe('azure-mobile-apps.data.mssql.statements', function () {
    describe('insert', function () {
        var insert = statements.insert;

        it('generates simple statement and parameters', function () {
            var statement = insert({ name: 'table' }, { id: 'id', p1: 'value', p2: 2.2 });
            expect(statement.sql).to.equal('INSERT INTO [dbo].[table] ([id],[p1],[p2]) VALUES (@id,@p1,@p2); SELECT * FROM [dbo].[table] WHERE [id] = @id');
            expect(statement.parameters).to.deep.equal([{ name: 'id', type: mssql.NVarChar(255), value: 'id' }, { name: 'p1', type: mssql.NVarChar(), value: 'value' }, { name: 'p2', type: mssql.Float, value: 2.2 }]);
        });

        it('does not specify id when table has autoIncrement specified', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id', p1: 'value' });
            expect(statement.sql).to.equal('INSERT INTO [dbo].[table] ([p1]) VALUES (@p1); SELECT * FROM [dbo].[table] WHERE [id] = SCOPE_IDENTITY()');
            expect(statement.parameters).to.deep.equal([{ name: 'p1', type: mssql.NVarChar(), value: 'value' }]);
        });

        it('ignores null values', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id', p1: null });
            expect(statement.sql).to.equal('INSERT INTO [dbo].[table] DEFAULT VALUES; SELECT * FROM [dbo].[table] WHERE [id] = SCOPE_IDENTITY()');
            expect(statement.parameters).to.deep.equal([]);
        });

        it('inserts zero values correctly', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id', p1: 0 });
            expect(statement.sql).to.equal('INSERT INTO [dbo].[table] ([p1]) VALUES (@p1); SELECT * FROM [dbo].[table] WHERE [id] = SCOPE_IDENTITY()');
            expect(statement.parameters).to.deep.equal([{ name: 'p1', type: mssql.Int, value: 0 }]);
        });

        it('inserts default values if none specified', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id' });
            expect(statement.sql).to.equal('INSERT INTO [dbo].[table] DEFAULT VALUES; SELECT * FROM [dbo].[table] WHERE [id] = SCOPE_IDENTITY()');
            expect(statement.parameters).to.deep.equal([]);
        });
    });
});
