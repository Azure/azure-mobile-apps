// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require("chai").expect,
    mssql = require("mssql"),
    statements = require("../../../../src/data/sqlite/statements");

describe("azure-mobile-apps.data.sqlite.statements", function () {
    describe("createTable", function () {
        var createTable = statements.createTable

        it("generates create statement with string id", function () {
            var statement = createTable({ name: "table" }, columns('string'));
            expect(statement.sql).to.equal("CREATE TABLE [table] ([id] TEXT PRIMARY KEY, createdAt TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'now')), updatedAt TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'now')), version TEXT NOT NULL DEFAULT 1, deleted INTEGER NOT NULL DEFAULT 0, [text] TEXT NULL)")
        });

        it("generates create statement with numeric id", function () {
            var statement = createTable({ name: "table" }, columns('number'));
            expect(statement.sql).to.equal("CREATE TABLE [table] ([id] INTEGER PRIMARY KEY, createdAt TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'now')), updatedAt TEXT NOT NULL DEFAULT (STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'now')), version TEXT NOT NULL DEFAULT 1, deleted INTEGER NOT NULL DEFAULT 0, [text] TEXT NULL)")
        });

        function columns(idType) {
            return [
                { name: 'id', type: idType },
                { name: 'createdAt', type: 'date' },
                { name: 'updatedAt', type: 'date' },
                { name: 'version', type: 'string' },
                { name: 'deleted', type: 'boolean' },
                { name: 'text', type: 'string' }
            ];
        }
    });
});
