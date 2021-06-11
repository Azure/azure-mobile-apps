// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var queries = require('../../query');

var apply = {
    filter: function (filter, query, context) {
        return require('./' + filter).filter(query, context) || query;
    },
    transform: function (transform, item, context) {
        return require('./' + transform).transform(item, context) || item;
    },
    hook: function (hook, results, context) {
        return require('./' + hook).hook(results, context);
    }
};

module.exports = {
    filters: function (table, query, context) {
        // apply requested builtins
        if(table.perUser) query = apply.filter('perUser', query, context);
        if(table.recordsExpire) query = apply.filter('recordsExpire', query, context);

        if(!table.filters) return query;

        // apply custom filters defined on table 
        return table.filters.reduce(function (query, filter) {
            return filter(query, context) || query;
        }, query || queries.create(table.name));
    },
    transforms: function (table, item, context) {
        if(table.perUser) item = apply.transform('perUser', item, context);

        if(!table.transforms) return item;

        return table.transforms.reduce(function (item, transform) {
            return transform(item, context) || item;
        }, item);
    },
    hooks: function (context) {
        var table = context.table;

        // hooks are post operation and are always called directly from a promise's .then callback - wrap in a function
        return function (results) {
            if(table && table.webhook) apply.hook('webhook', results, context);

            if(table && table.hooks) table.hooks.forEach(function (hook) {
                hook(results, context);
            });

            // currently, hooks are only fire and forget - no returning of modified results or waiting on async operations is supported
            return results;
        };
    }
};

