// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/sqlite/statements');

describe('azure-mobile-apps.data.sqlite.statements', function () {
    describe('createIndex', function () {
        var createIndex = statements.createIndex;

        it('generates simple statement', function () {
            var statement = createIndex({ name: 'table' }, 'foo');
            expect(statement.sql).to.equal('CREATE INDEX [foo] ON [table] ([foo])');
        });

        it('generates statement with multiple index columns', function () {
            var statement = createIndex({ name: 'table' }, ['foo', 'bar', 'baz']);
            expect(statement.sql).to.equal('CREATE INDEX [foo,bar,baz] ON [table] ([foo],[bar],[baz])');
        });
    });
});
