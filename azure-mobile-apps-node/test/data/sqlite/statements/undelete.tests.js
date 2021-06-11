// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').use(require('chai-subset')).expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/sqlite/statements'),
    queries = require('../../../../src/query');

describe('azure-mobile-apps.data.sqlite.statements', function () {
    describe('undelete', function () {
        it('generates simple statement and parameters', function () {
            var statement = statements.undelete({ name: 'table' }, queries.create('table').where({ id: 'id' }));
            expect(statement).to.containSubset([
                {
                    sql: "UPDATE [table] SET deleted = 0 WHERE ([id] = @p1)",
                    parameters: { p1: 'id' }
                },
                {
                    sql: "SELECT changes() AS recordsAffected",
                },
                {
                    sql: "SELECT * FROM [table] WHERE ([id] = @p1)",
                    parameters: { p1: 'id' }
                }
            ]);
        });
    });
});
