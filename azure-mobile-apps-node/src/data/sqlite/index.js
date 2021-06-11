// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var statements = require('./statements'),
    serializeModule = require('./serialize'),
    dynamicSchemaModule = require('./dynamicSchema'),
    schemaModule = require('./schema'),
    columnsModule = require('./columns'),
    connections = require('./connections'),
    log = require('../../logger'),
    queries = require('../../query'),
    uuid = require('uuid');

module.exports = function (configuration) {
    configuration = configuration || {};

    var connection = connections(configuration),
        serialize = serializeModule(connection),
        schema = schemaModule(connection, serialize),
        columns = columnsModule(connection, serialize);

    var tableAccess = function (table) {
        var dynamicSchema = dynamicSchemaModule(connection, table, serialize);

        // set execute functions based on dynamic schema and operation
        var read, update, insert, del;
        if (table.dynamicSchema !== false) {
            read = dynamicSchema.read;
            update = insert = del = dynamicSchema.execute;
        } else {
            read = update = insert = del = serialize;
        }

        return {
            read: function (query) {
                return columns.for(table).then(function () {
                    query = query || queries.create(table.containerName);
                    return read(statements.read(query, table));
                });
            },
            update: function (item, query) {
                return columns.for(table).then(function () {
                    return update(statements.update(table, item, query), item);
                });
            },
            insert: function (item) {
                return columns.for(table).then(function () {
                    item.id = item.id || uuid.v4();
                    return insert(statements.insert(table, item), item);
                });
            },
            delete: function (query, version) {
                return columns.for(table).then(function () {
                    return del(statements.delete(table, query, version));
                });
            },
            undelete: function (query, version) {
                return columns.for(table).then(function () {
                    return del(statements.undelete(table, query, version));
                });
            },
            truncate: function () {
                return serialize(statements.truncate(table));
            },
            initialize: function () {
                return schema.initialize(table).catch(function (error) {
                    log.error('Error occurred during table initialization', error);
                    throw error;
                });
            },
            schema: function () {
                return schema.get(table);
            }
        };
    };

    // expose a method to allow direct execution of SQL queries
    tableAccess.execute = function (statement) {
        return serialize(statement);
    };

    return tableAccess;
};
