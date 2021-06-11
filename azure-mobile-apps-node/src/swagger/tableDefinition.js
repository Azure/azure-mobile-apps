// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = function (configuration) {
    return function (table, schema) {
        return {
            type: 'object',
            properties: properties()
        };

        function properties() {
            return schema.properties.reduce(function (target, property) {
                target[property.name] = mapProperty(property);
                return target;
            }, {});
        }

        function mapProperty(property) {
            return {
                type: ({
                    'number': 'number',
                    'string': 'string',
                    'boolean': 'boolean',
                    'date': 'string',
                    'datetime': 'string'
                })[property.type],
                format: ({
                    'number': 'float',
                    'string': undefined,
                    'boolean': undefined,
                    'date': 'date-time',
                    'datetime': 'date-time'
                })[property.type],
                // not sure why this is causing validation failures
                //required: (property.name === 'id') || undefined,
                readOnly: (['createdAt', 'updatedAt', 'version'].indexOf(property.name) > -1) || undefined
            }
        }
    };
};
