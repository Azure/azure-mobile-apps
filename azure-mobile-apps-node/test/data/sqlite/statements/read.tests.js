// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    queries = require('../../../../src/query'),
    statements = require('../../../../src/data/sqlite/statements');

describe('azure-mobile-apps.data.sqlite.statements', function () {
    describe('read', function () {
        it("preserves simple statement", function () {
            var statement = statements.read(queries.create('table').where({ p1: 1 }), { name: 'table' });
            expect(statement.sql).to.equal('SELECT * FROM [table] WHERE ([p1] = @p1)');
            expect(statement.parameters).to.deep.equal({ p1: 1 });
        });

        it("combines multiple statements into single statement", function () {
            var query = queries.create('table').includeTotalCount(),
                statement = statements.read(query, { name: 'table' });

            expect(statement).to.containSubset([
                {
                    sql: 'SELECT * FROM [table]',
                    parameters: { }
                },
                {
                    sql: 'SELECT COUNT(*) AS [count] FROM [table]'
                }
            ]);
        });
    });
});
