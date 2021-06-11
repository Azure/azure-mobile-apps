// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// there should be more tests here, but SQL tests can be fragile
// full coverage should be provided by integration tests

var expect = require('chai').expect,
    mssql = require('mssql'),
    statements = require('../../../../src/data/mssql/statements');

describe('azure-mobile-apps.data.mssql.statements', function () {
    describe('createTable', function () {
        var createTable = statements.createTable;

        it('generates create statement with string id', function () {
            var statement = createTable({ name: 'table' }, { id: '1', text: 'test' });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] NVARCHAR(255) NOT NULL PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),[text] NVARCHAR(MAX) NULL) ON [PRIMARY]')
        });

        it('generates create statement with numeric id', function () {
            var statement = createTable({ name: 'table' }, { id: 1, text: 'test' });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] INT NOT NULL PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),[text] NVARCHAR(MAX) NULL) ON [PRIMARY]')
        });

        it('generates create statement with string id if none is provided', function () {
            var statement = createTable({ name: 'table' }, { text: 'test' });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] NVARCHAR(255) NOT NULL PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),[text] NVARCHAR(MAX) NULL) ON [PRIMARY]')
        });

        it('generates integer identity column when autoIncrement is specified', function () {
            var statement = createTable({ name: 'table', autoIncrement: true }, { text: 'test' });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] INT NOT NULL IDENTITY (1, 1) PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),[text] NVARCHAR(MAX) NULL) ON [PRIMARY]')
        });

        it('generates create statement with deleted column', function () {
            var statement = createTable({ name: 'table', softDelete: true }, { id: '1', text: 'test' });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] NVARCHAR(255) NOT NULL PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),deleted bit NOT NULL DEFAULT 0,[text] NVARCHAR(MAX) NULL) ON [PRIMARY]')
        });

        it('generates create statement with predefined columns', function () {
            var statement = createTable({ name: 'table', columns: { number: 'number' } }, { text: 'test' });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] NVARCHAR(255) NOT NULL PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),[text] NVARCHAR(MAX) NULL,[number] FLOAT(53)) ON [PRIMARY]')
        });

        it('generates create statement without an item', function () {
            var statement = createTable({ name: 'table', columns: { number: 'number' } });
            expect(statement.sql).to.equal('CREATE TABLE [dbo].[table] ([id] NVARCHAR(255) NOT NULL PRIMARY KEY,version ROWVERSION NOT NULL,createdAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),updatedAt DATETIMEOFFSET(7) NOT NULL DEFAULT CONVERT(DATETIMEOFFSET(7),SYSUTCDATETIME(),0),[number] FLOAT(53)) ON [PRIMARY]')
        });
    });
});
