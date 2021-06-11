// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/query
@description Functionality for creating {@link https://github.com/Azure/azure-query-js azure-query-js} objects
*/
var Query = require('azure-query-js').Query,
    assert = require('../utilities/assert').argument;

module.exports = {
    /**
    Creates a new query against the specified table
    @param {string} table - Name of the table to query
    */
    create: function (table) {
        assert(table, 'A table name was not specified');
        return new Query(table);
    },
    /**
    Creates a query from an HTTP request object that encapsulates an OData query
    @param {express.Request} req The HTTP request object
    */
    fromRequest: function(req) {
        var url = req.baseUrl;
        return Query.Providers.OData.fromOData(
            url.substring(url.lastIndexOf('/') + 1),
            req.query.$filter,
            req.query.$orderby,
            parseInt(req.query.$skip),
            parseInt(req.query.$top),
            req.query.$select,
            req.query.$inlinecount === 'allpages',
            !!req.query.__includeDeleted)
    },
    /**
    Converts a query to an object containing OData query information
    @param {module:queryjs/Query} query - The azure-query-js Query object to convert
    */
    toOData: function (query) {
        return Query.Providers.OData.toOData(query)
    },
    Query: Query
}
