// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var ODataParameters = require('./ODataParameters');

module.exports = function (configuration, schema, definition) {
    var createOperation = function (options) { //summary, description, parameters, odata, responses, operation
        options.parameters = options.parameters || [];
        options.responses = options.responses || {};

        options.responses['400'] = createResponse('The format of the request was incorrect', 'error');
        options.responses['404'] = createResponse('The table or item could not be found', 'error');
        options.responses['405'] = createResponse('The operation has been disabled', 'error');
        options.responses['500'] = createResponse('An internal error occurred', 'error');

        var operation = {
            tags: [schema.name],
            summary: options.summary,
            description: options.description,
            parameters: options.parameters.map(createParameter),
            operationId: options.operation + camelCase(schema.name),
            responses: options.responses,
            security: security(options.operation)
        };

        if(options.odata)
            operation.parameters = operation.parameters.concat(ODataParameters);

        operation.parameters = operation.parameters.concat(createParameter('apiVersion'));

        return operation;
    };

    createOperation.response = createResponse;

    return createOperation;

    function createParameter(name) {
        return ({
            'id': {
                name: "id",
                description: "The record identifier",
                required: true,
                type: "string",
                in: "path"
            },
            'body': {
                name: "body",
                description: "The item",
                required: true,
                schema: {
                    $ref: "#/definitions/" + schema.name
                },
                in: "body"
            },
            'apiVersion': {
                name: "zumo-api-version",
                description: "The Azure Mobile Apps API version",
                required: true,
                type: "string",
                in: "header",
                default: configuration.apiVersion
            }
        })[name];
    }

    function createResponse(description, type) {
        return {
            description: description,
            schema: getSchema()
        };

        function getSchema() {
            return ({
                'item': { $ref: '#/definitions/' + schema.name },
                'array': { type: 'array', items: { $ref: '#/definitions/' + schema.name } },
                'error': undefined //{ $ref: '#/definitions/errorType' }
            })[type];
        }
    }

    function camelCase(name) {
        return name[0].toUpperCase() + name.substring(1);
    }

    function security(operation) {
        var definitions = [];
        if(operationAccess(operation) === 'authenticated')
            definitions.push({"EasyAuth": []});
        return definitions;
    }

    function operationAccess(operation) {
        var tableOperationName = tableOperation(operation),
            operationAccessLevel = definition[tableOperationName] && definition[tableOperationName].access;
        
        return operationAccessLevel || definition.access || 'anonymous';
    }

    function tableOperation(operation) {
        switch(operation) {
            case 'Find':
            case 'Query':
                return 'read';
            case 'Insert':
                return 'insert';
            case 'Update':
                return 'update';
            case 'Delete':
                return 'delete';
            case 'Undelete':
                return 'undelete';
        }
    }
};
