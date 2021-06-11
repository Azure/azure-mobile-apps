// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var promises = require('../utilities/promises'),
    log = require('../logger');

module.exports = function (tables) {
    log.warn('The memory data provider is deprecated and will be removed from future versions. Use the sqlite provider instead.');

    tables = tables || {};

    return function (table) {
        return {
            read: function (query) {
                // need to evaluate query against each item before returning
                return promises.resolved(values(table));
            },
            update: function (item) {
                items(table)[item.id] = item;
                return promises.resolved(item);
            },
            insert: function (item) {
                items(table)[item.id] = item;
                return promises.resolved(item);
            },
            delete: function (id, version) {
                delete items(table)[id];
                return promises.resolved(id);
            },
            undelete: function (id) {
                // unsupported
                return promises.resolved({ id: id });
            },
            truncate: function () {
                tables[table.name] = {};
                return promises.resolved();
            },
            initialize: function () {
                return promises.resolved();
            }
        }

        function items(table) {
            var name = table.name.toLowerCase();
            if(!tables[name])
                tables[name] = {};
            return tables[name];
        }

        function values(table) {
            var tableItems = items(table);
            return Object.keys(tableItems).map(function (id) {
                return tableItems[id];
            });
        }
    };
};
