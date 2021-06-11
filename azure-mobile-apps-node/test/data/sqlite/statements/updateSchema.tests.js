// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require("chai").use(require("chai-subset")).expect,
    mssql = require("mssql"),
    statements = require("../../../../src/data/sqlite/statements");

describe("azure-mobile-apps.data.sqlite.statements", function () {
    describe("updateSchema", function () {
        var updateSchema = statements.updateSchema,
            table = { name: 'table' },
            base = [
                { name: 'id', type: 'string' },
                { name: 'createdAt', type: 'date' },
                { name: 'updatedAt', type: 'date' },
                { name: 'version', type: 'string' },
                { name: 'deleted', type: 'boolean' }
            ];

        it("generates noop if no new columns are specified", function () {
            var statement = updateSchema(table, base, base);
            expect(statement.noop).to.be.true;
        })

        it("generates single statement", function () {
            var statement = updateSchema(table, base, base.concat({ name: 'text', type: 'string' }));
            expect(statement.length).to.equal(1);
            expect(statement[0].sql).to.equal("ALTER TABLE [table] ADD COLUMN [text] TEXT NULL");
        });

        it("generates multiple statements", function () {
            var statement = updateSchema(table, base, base.concat([{ name: 'text', type: 'string' }, { name: 'completed', type: 'boolean' }]));
            expect(statement.length).to.equal(2);
            expect(statement[0].sql).to.equal("ALTER TABLE [table] ADD COLUMN [text] TEXT NULL");
            expect(statement[1].sql).to.equal("ALTER TABLE [table] ADD COLUMN [completed] INTEGER NULL");
        })
    });
});
