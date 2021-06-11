// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/sqlite/statements');

describe('azure-mobile-apps.data.sqlite.statements', function () {
    describe('insert', function () {
        var insert = statements.insert;

        it('generates simple statement and parameters', function () {
            var statement = insert({ name: 'table' }, { id: 'id', p1: 'value', p2: 2.2 });
            expect(statement.length).to.equal(2);
            expect(statement[0].sql).to.equal('INSERT INTO [table] ([id],[p1],[p2]) VALUES (@id,@p1,@p2);');
            expect(statement[0].parameters).to.deep.equal({ id: 'id', p1: 'value', p2: 2.2 });
            expect(statement[1].sql).to.equal('SELECT * FROM [table] WHERE [rowid] = last_insert_rowid();');
        });

        it('does not specify id when table has autoIncrement specified', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id', p1: 'value' });
            expect(statement[0].sql).to.equal('INSERT INTO [table] ([p1]) VALUES (@p1);');
            expect(statement[0].parameters).to.deep.equal({ p1: 'value' });
        });

        it('ignores null values', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id', p1: null });
            expect(statement[0].sql).to.equal('INSERT INTO [table] DEFAULT VALUES;');
            expect(statement[0].parameters).to.deep.equal({ });
        });

        it('inserts zero values correctly', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id', p1: 0 });
            expect(statement[0].sql).to.equal('INSERT INTO [table] ([p1]) VALUES (@p1);');
            expect(statement[0].parameters).to.deep.equal({ p1: 0 });
        });

        it('inserts default values if none specified', function () {
            var statement = insert({ name: 'table', autoIncrement: true }, { id: 'id' });
            expect(statement[0].sql).to.equal('INSERT INTO [table] DEFAULT VALUES;');
            expect(statement[0].parameters).to.deep.equal({});
        });
    });
});
