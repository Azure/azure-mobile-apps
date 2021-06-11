// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var config = require('../../appFactory').configuration().data,
    dynamicSchema = require('../../../src/data/mssql/dynamicSchema'),
    statements = require('../../../src/data/mssql/statements'),
    execute = require('../../../src/data/mssql/execute'),
    queries = require('../../../src/query'),
    promises = require('../../../src/utilities/promises'),
    helpers = require('../../../src/data/mssql/helpers'),
    expect = require('chai').use(require('chai-subset')).use(require('chai-as-promised')).expect,
    table = { name: 'dynamicSchema', softDelete: true };

// these tests rely on specific queries like getColumns and getIndexes from the mssql provider
if(config.provider === 'mssql') {
    describe('azure-mobile-apps.data.mssql.integration.dynamicSchema', function () {
        afterEach(function (done) {
            execute(config, { sql: 'drop table dbo.dynamicSchema' }).then(done, done);
        });

        it("creates basic table schema", function () {
            var item = { id: '1' };
            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, statements.getColumns(table));
                })
                .then(function (columns) {
                    expect(columns).to.deep.equal([
                        { name: 'id', type: 'nvarchar' },
                        { name: 'version', type: 'timestamp' },
                        { name: 'createdAt', type: 'datetimeoffset' },
                        { name: 'updatedAt', type: 'datetimeoffset' },
                        { name: 'deleted', type: 'bit' }
                    ]);
                });
        });

        it("creates table and schema", function () {
            var item = { id: '1', string: 'test', number: 1, boolean: true };
            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, statements.getColumns(table));
                })
                .then(function (columns) {
                    expect(columns).to.deep.equal([
                        { name: 'id', type: 'nvarchar' },
                        { name: 'version', type: 'timestamp' },
                        { name: 'createdAt', type: 'datetimeoffset' },
                        { name: 'updatedAt', type: 'datetimeoffset' },
                        { name: 'deleted', type: 'bit' },
                        { name: 'string', type: 'nvarchar' },
                        { name: 'number', type: 'float' },
                        { name: 'boolean', type: 'bit' }
                    ]);
                });
        });

        it("updates schema", function () {
            var item = { id: '1', string: 'test', number: 1, boolean: true };
            return dynamicSchema(table).execute(config, statements.insert(table, { id: '1' }), { id: '1' })
                .then(function () {
                    return dynamicSchema(table).execute(config, statements.update(table, item), item);
                })
                .then(function () {
                    return execute(config, statements.getColumns(table));
                })
                .then(function (columns) {
                    expect(columns).to.deep.equal([
                        { name: 'id', type: 'nvarchar' },
                        { name: 'version', type: 'timestamp' },
                        { name: 'createdAt', type: 'datetimeoffset' },
                        { name: 'updatedAt', type: 'datetimeoffset' },
                        { name: 'deleted', type: 'bit' },
                        { name: 'string', type: 'nvarchar' },
                        { name: 'number', type: 'float' },
                        { name: 'boolean', type: 'bit' }
                    ]);
                });
        });

        it("creates primary key constraint", function () {
            var item = { id: '1' };
            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, { sql: "SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'dynamicSchema'" });
                })
                .then(function (constraints) {
                    expect(constraints.length).to.equal(1);
                    expect(constraints[0].CONSTRAINT_NAME.indexOf('PK__dynamicS')).to.not.equal(-1);
                });
        });

        it("creates table and schema with numeric id", function () {
            var item = { id: 1, string: 'test', number: 1, boolean: true };
            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, statements.getColumns(table));
                })
                .then(function (columns) {
                    expect(columns).to.deep.equal([
                        { name: 'id', type: 'int' },
                        { name: 'version', type: 'timestamp' },
                        { name: 'createdAt', type: 'datetimeoffset' },
                        { name: 'updatedAt', type: 'datetimeoffset' },
                        { name: 'deleted', type: 'bit' },
                        { name: 'string', type: 'nvarchar' },
                        { name: 'number', type: 'float' },
                        { name: 'boolean', type: 'bit' }
                    ]);
                });
        });

        it("creates table with autoIncrement identity if specified", function () {
            var item = { value: 'test' },
                autoIncrementTable = { name: 'dynamicSchema', autoIncrement: true };
            return dynamicSchema(autoIncrementTable).execute(config, statements.insert(autoIncrementTable, item), item)
                .then(function () {
                    return execute(config, statements.read(queries.create('dynamicSchema')));
                })
                .then(function (results) {
                    expect(results).to.containSubset([{ id: 1, value: 'test' }]);
                });
        });

        it("creates insert/update/delete trigger", function () {
            var item = { id: '1' };
            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, { sql: "SELECT * FROM sys.triggers WHERE name = 'TR_dynamicSchema_InsertUpdateDelete'" });
                })
                .then(function (triggers) {
                    expect(triggers.length).to.equal(1);
                });
        });

        it("updates updatedAt column", function () {
            var item = { id: '1', string: 'test', number: 1, boolean: true },
                updatedAt;

            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function (inserted) {
                    updatedAt = inserted.updatedAt;
                    // I don't quite understand why the test is occasionally flaky. Suspect datetimeoffset resolution in SQL Server.
                    return promises.sleep();
                })
                .then(function () {
                    return dynamicSchema(table).execute(config, statements.update(table, item), item);
                })
                .then(function (updated) {
                    expect(updated.updatedAt).to.be.greaterThan(updatedAt);
                });
        });

        it("creates predefined columns", function () {
            var table = { name: 'dynamicSchema', softDelete: true, columns: {
                    string: 'string',
                    number: 'number',
                    bool: 'boolean',
                    date: 'datetime'
                } },
                item = { id: '1' };

            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, statements.getColumns(table));
                })
                .then(function (columns) {
                    expect(columns).to.deep.equal([
                        { name: 'id', type: 'nvarchar' },
                        { name: 'version', type: 'timestamp' },
                        { name: 'createdAt', type: 'datetimeoffset' },
                        { name: 'updatedAt', type: 'datetimeoffset' },
                        { name: 'deleted', type: 'bit' },
                        { name: 'string', type: 'nvarchar' },
                        { name: 'number', type: 'float' },
                        { name: 'bool', type: 'bit' },
                        { name: 'date', type: 'datetimeoffset' }
                    ]);
                });
        });

        it("uses predefined column types over item types when duplicated with different casing", function () {
            var table = { name: 'dynamicSchema', softDelete: true, columns: { string: 'string' } },
                item = { id: '1', String: 1 };

            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, statements.getColumns(table));
                })
                .then(function (columns) {
                    expect(columns).to.deep.equal([
                        { name: 'id', type: 'nvarchar' },
                        { name: 'version', type: 'timestamp' },
                        { name: 'createdAt', type: 'datetimeoffset' },
                        { name: 'updatedAt', type: 'datetimeoffset' },
                        { name: 'deleted', type: 'bit' },
                        { name: 'string', type: 'nvarchar' }
                    ]);
                });
        });

        it("creates predefined indexes", function () {
            var table = {
                name: 'dynamicSchema',
                columns: {
                    string: 'string',
                    blah: 'number',
                    num: 'number',
                    bool: 'boolean'
                },
                indexes : [
                    'blah',
                    ['bool', 'num'],
                    ['blah', 'bool', 'num']
                ]
            },
                item = { id: '1'};

            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function() {
                    return execute(config, statements.getIndexes(table));
                })
                .then(function (indexesInfo) {
                    expect(transformIndexInfo(indexesInfo)).to.containSubset(transformIndexConfig(table.indexes));
                });
        });

        it("throws error when creating index with unsupported column type", function () {
            var table = {
                name: 'dynamicSchema',
                columns: {
                    string: 'string'
                },
                indexes : [
                    'string'
                ]
            },
                item = { id: '1'};

            return expect(dynamicSchema(table).execute(config, statements.insert(table, item), item))
                .to.be.rejectedWith('Column \'string\' in table \'dbo.dynamicSchema\' is of a type that is invalid for use as a key column in an index.');
        });

        it("throws error when creating index on column that does not exist", function () {
            var table = {
                name: 'dynamicSchema',
                columns: {
                    blah: 'number'
                },
                indexes : [
                    'foo'
                ]
            },
                item = { id: '1'};

            return expect(dynamicSchema(table).execute(config, statements.insert(table, item), item))
                .to.be.rejectedWith('Column name \'foo\' does not exist in the target table or view.');
        });

        it("throws error when index columns is not an array", function () {
            var table = {
                name: 'dynamicSchema',
                columns: {
                    blah: 'number'
                },
                indexes : [
                    {}
                ]
            },
                item = { id: '1'};

            return expect(dynamicSchema(table).execute(config, statements.insert(table, item), item))
                .to.be.rejectedWith('Index configuration of table \'' + table.name + '\' should be an array containing either strings or arrays of strings.');

        });

        it("throws error when indexes config is not an array", function () {
            var table = {
                name: 'dynamicSchema',
                columns: {
                    blah: 'number'
                },
                indexes : {}
            },
                item = { id: '1'};

            return expect(dynamicSchema(table).execute(config, statements.insert(table, item), item))
                .to.be.rejectedWith('Index configuration of table \'' + table.name + '\' should be an array containing either strings or arrays of strings.');
        });

        it("creates tables with specified database table name", function () {
            var item = { id: '1' },
                table = {
                    name: 'dynamicSchemaTable',
                    databaseTableName: 'dynamicSchema'
                };

            return dynamicSchema(table).execute(config, statements.insert(table, item), item)
                .then(function () {
                    return execute(config, statements.read(queries.create('dynamicSchema'), table));
                })
                .then(function (rows) {
                    expect(rows.length).to.equal(1);
                });
        });
    });

    function transformIndexInfo(indexInfo) {
        return indexInfo.map(function (index) {
            return index.index_keys;
        });
    };

    function transformIndexConfig(config) {
        return config.map(function (index) {
            if (Array.isArray(index)) {
                return index.join(', ');
            } else {
                return index;
            }
        });
    };
}
