// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').expect,
    queries = require('../../../../src/query'),
    statements = require('../../../../src/data/mssql/statements');

describe('azure-mobile-apps.data.mssql.statements', function () {
    describe('read', function () {
        it("preserves simple statement", function () {
            var statement = statements.read(queries.create('table').where({ p1: 1 }), { name: 'table' });
            expect(statement.sql).to.equal('SELECT * FROM [dbo].[table] WHERE ([p1] = @p1); ');
        });

        it("combines multiple statements into single statement", function () {
            var query = queries.create('table').includeTotalCount(),
                statement = statements.read(query, { name: 'table' });
            expect(statement.sql).to.equal('SELECT * FROM [dbo].[table]; SELECT COUNT(*) AS [count] FROM [dbo].[table]; ');
        });
    });
});
