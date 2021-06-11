// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/mssql/statements');

describe('azure-mobile-apps.data.mssql.statements', function () {
    describe('updateSchema', function () {
        var updateSchema = statements.updateSchema;

        it('generates simple statement', function () {
            var statement = updateSchema({ name: 'table' }, [{ name: 'id' }, { name: 'version' }, { name: 'createdAt' }, { name: 'updatedAt' }, { name: 'deleted' }], { id: 1, text: 'test' });
            expect(statement.sql).to.equal('ALTER TABLE [dbo].[table] ADD [text] NVARCHAR(MAX) NULL');
        });

        it('generates system properties if missing', function () {
            var statement = updateSchema({ name: 'table' }, [{ name: 'id' }], { id: 1, text: 'test' });
            expect(statement.sql).to.equal('ALTER TABLE [dbo].[table] ADD [text] NVARCHAR(MAX) NULL,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0)');
        });

        it('generates deleted column if specified and missing', function () {
            var statement = updateSchema({ name: 'table', softDelete: true }, [{ name: 'id' }], { id: 1, text: 'test' });
            expect(statement.sql).to.equal('ALTER TABLE [dbo].[table] ADD [text] NVARCHAR(MAX) NULL,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),deleted bit NOT NULL DEFAULT 0');
        });

        it('correctly handles missing system properties that exist in item', function () {
            var statement = updateSchema({ name: 'table' }, [{ name: 'id' }, { name: 'createdAt' }, { name: 'updatedAt' }, { name: 'deleted' }], { id: 1, text: 'test', version: 'someVersion' });
            expect(statement.sql).to.equal('ALTER TABLE [dbo].[table] ADD [text] NVARCHAR(MAX) NULL,version ROWVERSION NOT NULL');
        });

        it('generates statement for predefined columns', function () {
            var statement = updateSchema(
                { name: 'table', columns: { 'text': 'string' } },
                [{ name: 'id' }, { name: 'createdAt' }, { name: 'updatedAt' }, { name: 'deleted' }, { name : 'version' }],
                { id: 1 }
            );
            expect(statement.sql).to.equal('ALTER TABLE [dbo].[table] ADD [text] NVARCHAR(MAX) NULL');
        });

        it('generates statement for predefined columns when item is not supplied', function () {
            var statement = updateSchema(
                { name: 'table', columns: { 'text': 'string' } },
                [{ name: 'id' }, { name: 'createdAt' }, { name: 'updatedAt' }, { name: 'deleted' }, { name : 'version' }]
            );
            expect(statement.sql).to.equal('ALTER TABLE [dbo].[table] ADD [text] NVARCHAR(MAX) NULL');
        });
    });
});
