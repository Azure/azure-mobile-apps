// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var expect = require('chai').use(require('chai-subset')).expect,
    swagger = require('../../src/swagger'),
    paths = require('../../src/swagger/tablePaths'),
    definition = require('../../src/swagger/tableDefinition'),

    table = { name: 'todoitem', databaseTableName: 'todoitemTable' },
    schema = {
        name: 'todoitem',
        properties: [
            { name: 'number', type: 'number' },
            { name: 'boolean', type: 'boolean' },
            { name: 'string', type: 'string' },
            { name: 'datetime', type: 'datetime' },
        ]
    },
    configuration = {
        tableRootPath: '/tables',
        tables: { todoitem: table }
    };

describe('azure-mobile-apps.swagger', function () {
    it("generates basic swagger structure", function () {
        expect(swagger(configuration)('/', 'localhost', [schema])).to.containSubset({
            "swagger": "2.0",
            "host": "localhost",
            "basePath": "/",
            "info": {},
            "tags": {},
            "paths": {},
            "definitions": { "errorType": {} },
            "securityDefinitions": { "EasyAuth": {} }
        });
    });

    describe('tables.path', function () {
        it("generates path objects for tables", function () {
            expect(paths(configuration)(schema, table)).to.containSubset({
                '/tables/todoitem': {
                    get: { parameters: [ { name: "$filter" }, { name: "zumo-api-version" } ], operationId: 'QueryTodoitem', responses: {} },
                    post: { parameters: [ { in: 'body' }, { name: "zumo-api-version" } ], operationId: 'InsertTodoitem', responses: {} }
                },
                '/tables/todoitem/{id}': {
                    get: { parameters: [ { name: "id" }, { name: "zumo-api-version" } ], operationId: 'FindTodoitem', responses: {} },
                    post: { parameters: [ { name: "id" }, { name: "zumo-api-version" } ], operationId: 'UndeleteTodoitem', responses: {} },
                    patch: { parameters: [ { name: "id" }, { in: 'body' }, { name: "zumo-api-version" } ], operationId: 'UpdateTodoitem', responses: {}},
                    delete: { parameters: [ { name: "id" }, { name: "zumo-api-version" } ], operationId: 'DeleteTodoitem', responses: {} }
                }
            });
        });

        it("adds security attribute to individual operations", function () {
            var swagger = paths(configuration)(schema, { name: 'todoitem', insert: { access: 'authenticated' } });
            expect(swagger['/tables/todoitem'].get.security).to.deep.equal([]);
            expect(swagger['/tables/todoitem'].post.security).to.deep.equal([{ EasyAuth: [] }]);
        });

        it("adds security attribute to tables", function () {
            var swagger = paths(configuration)(schema, { name: 'todoitem', access: 'authenticated' });
            expect(swagger['/tables/todoitem'].get.security).to.deep.equal([{ EasyAuth: [] }]);
            expect(swagger['/tables/todoitem'].post.security).to.deep.equal([{ EasyAuth: [] }]);
        });

        it("individual operations override table level access", function () {
            var swagger = paths(configuration)(schema, { name: 'todoitem', access: 'authenticated', read: { access: 'anonymous' } });
            expect(swagger['/tables/todoitem'].get.security).to.deep.equal([]);
            expect(swagger['/tables/todoitem'].post.security).to.deep.equal([{ EasyAuth: [] }]);
        });
    });

    describe('tables.definition', function () {
        it("generates definition object for tables from columns", function () {
            expect(definition(configuration)(table, schema)).to.containSubset({
                type: 'object',
                properties: {
                    'number': { type: 'number', format: 'float' },
                    'boolean': { type: 'boolean' },
                    'string': { type: 'string' },
                    'datetime': { type: 'string', format: 'date-time' },
                }
            });
        });
    });
});
