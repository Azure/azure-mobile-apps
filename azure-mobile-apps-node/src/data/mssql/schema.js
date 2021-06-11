// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
module.exports = function (configuration) {
    // these require statements must appear within this function to avoid circular reference issues between dynamicSchema and schema
    var statements = require('./statements'),
        execute = require('./execute'),
        dynamicSchema = require('./dynamicSchema'),
        promises = require('../../utilities/promises'),
        log = require('../../logger'),
        helpers = require('./helpers'),
        uuid = require('uuid');

    var api = {
        initialize: function (table) {
            return execute(configuration, statements.getColumns(table))
                .then(function (columns) {
                    if(columns.length === 0)
                        return api.createTable(table)
                            .catch(function (error) {
                                log.error("Error occurred creating table " + table.name + ":", error);
                                throw error;
                            });
                    else
                        return api.updateSchema(table);
                });
        },

        createTable: function(table, item) {
            log.info('Creating table ' + table.name);
            return execute(configuration, statements.createTable(table, item))
                .then(function () {
                    return execute(configuration, statements.createTrigger(table));
                })
                .then(function () {
                    return api.createIndexes(table);
                })
                .then(function () {
                    return api.seedData(table);
                });
        },

        updateSchema: function(table, item) {
            log.info('Updating schema for table ' + table.name);
            return execute(configuration, statements.getColumns(table))
                .then(function (columns) {
                    return execute(configuration, statements.updateSchema(table, columns, item));
                })
                .then(function () {
                    return api.createIndexes(table);
                });
        },

        createIndexes: function(table) {
            if(table.indexes) {
                if(Array.isArray(table.indexes)) {
                    log.info('Creating indexes for table ' + table.name);
                    return promises.all(
                        table.indexes.map(function (indexConfig) {
                            return execute(configuration, statements.createIndex(table, indexConfig));
                        })
                    );
                } else {
                    throw new Error('Index configuration of table \'' + table.name + '\' should be an array containing either strings or arrays of strings.');
                }
            } else {
                return promises.resolved();
            }
        },

        seedData: function (table) {
            return promises.series(table.seed, insert);

            function insert(item) {
                item.id = item.id || uuid.v4();
                return dynamicSchema(table).execute(configuration, statements.insert(table, item), item);
            }
        },

        get: function (table) {
            return execute(configuration, statements.getColumns(table)).then(function (columns) {
                return {
                    name: table.name,
                    properties: columns.map(function (column) {
                        return {
                            name: column.name,
                            type: helpers.getPredefinedType(column.type)
                        };
                    }),
                    definition: table
                };
            });
        }
    };
    return api;
}
