// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var tableOperation = require('./tableOperation');

module.exports = function (configuration) {
    return function (schema, definition) {
        var createOperation = tableOperation(configuration, schema, definition),
            paths = {};

        paths['/tables/' + schema.name] = {
            get: createOperation({
                summary: 'Query the ' + schema.name + ' table',
                description: 'The provided OData query is evaluated and an array of ' + schema.name + ' objects is returned. If no OData query is specified, all items are returned.',
                odata: true,
                operation: 'Query',
                responses: {
                    '200': createOperation.response('An array of items matching the provided query', 'array')
                }
            }),
            post: createOperation({
                summary: 'Insert a record into the ' + schema.name + ' table',
                parameters: ['body'],
                operation: 'Insert',
                responses: {
                    '201': createOperation.response('The inserted item', 'item'),
                    '409': createOperation.response('An item with the same ID already exists', 'item')
                }
            })
        }

        paths['/tables/' + schema.name + '/{id}'] = {
            get: createOperation({
                summary: 'Find a specific record in the ' + schema.name + ' table',
                description: 'Return the ' + schema.name + ' object that corresponds with the provided id.',
                parameters: ['id'],
                operation: 'Find',
                responses: {
                    '200': createOperation.response('The request item', 'item')
                }
            }),
            post: createOperation({
                summary: 'Undelete a record from the ' + schema.name + ' table',
                parameters: ['id'],
                operation: 'Undelete',
                responses: {
                    '201': createOperation.response('The undeleted item', 'item'),
                    '409': createOperation.response('A concurrency violation occurred', 'item'),
                    '412': createOperation.response('A concurrency violation occurred', 'item')
                }
            }),
            patch: createOperation({
                summary: 'Update a record in the ' + schema.name + ' table',
                parameters: ['id', 'body'],
                operation: 'Update',
                responses: {
                    '200': createOperation.response('The updated item', 'item'),
                    '409': createOperation.response('A concurrency violation occurred', 'item'),
                    '412': createOperation.response('A concurrency violation occurred', 'item')
                }
            }),
            delete: createOperation({
                summary: 'Delete a record from the ' + schema.name + ' table',
                parameters: ['id'],
                operation: 'Delete',
                responses: {
                    '200': createOperation.response('The deleted item', 'item'),
                    '409': createOperation.response('A concurrency violation occurred', 'item'),
                    '412': createOperation.response('A concurrency violation occurred', 'item')
                }
            })
        };

        return paths;
    }
}
