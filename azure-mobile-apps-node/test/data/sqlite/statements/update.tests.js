// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    queries = require('../../../../src/query'),
    statements = require('../../../../src/data/sqlite/statements');

describe('azure-mobile-apps.data.sqlite.statements', function () {
    describe('update', function () {
        var update = statements.update;

        it('generates simple statement and parameters', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value', p2: 2.2 });
            expect(statement[0].sql).to.equal('UPDATE [table] SET [p1] = @p1,[p2] = @p2 WHERE [id] = @id');
            expect(statement[0].parameters).to.deep.equal({ p1: 'value', p2: 2.2, id: 'id' });
            expect(statement[1].sql).to.equal('SELECT changes() AS recordsAffected');
            expect(statement[2].sql).to.equal('SELECT * FROM [table] WHERE [id] = @id');
            expect(statement[2].parameters).to.deep.equal({ id: 'id' })
        });

        it('updates null values correctly', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: null });
            expect(statement[0].sql).to.equal('UPDATE [table] SET [p1] = @p1 WHERE [id] = @id');
            expect(statement[0].parameters).to.deep.equal({ p1: null, id: 'id' });
        });

        it('updates zero values correctly', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 0 });
            expect(statement[0].sql).to.equal('UPDATE [table] SET [p1] = @p1 WHERE [id] = @id');
            expect(statement[0].parameters).to.deep.equal({ p1: 0, id: 'id' });
        });

        it('creates concurrency clause', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value', p2: 2.2, version: 'MQ==' });
            expect(statement[0].sql).to.equal('UPDATE [table] SET [p1] = @p1,[p2] = @p2 WHERE [id] = @id AND [version] = @version');
            expect(statement[0].parameters).to.deep.equal({ p1: 'value', p2: 2.2, id: 'id', version: '1' });
        });

        it('adds queries to where clause', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value' }, queries.create('table').where({ p2: 2 }));
            expect(statement[0].sql).to.equal('UPDATE [table] SET [p1] = @p1 WHERE [id] = @id AND ([p2] = @q1)');
            expect(statement[0].parameters).to.deep.equal({ p1: 'value', q1: 2, id: 'id' });
            expect(statement[2].sql).to.equal('SELECT * FROM [table] WHERE [id] = @id AND ([p2] = @q1)');
            expect(statement[2].parameters).to.deep.equal({ q1: 2, id: 'id' })
        });

        it('does not add empty queries to where clause', function () {
            var statement = update({ name: 'table' }, { id: 'id', p1: 'value' }, queries.create('table'));
            expect(statement[0].sql).to.equal('UPDATE [table] SET [p1] = @p1 WHERE [id] = @id');
            expect(statement[0].parameters).to.deep.equal({ p1: 'value', id: 'id' });
        });
    });
});
