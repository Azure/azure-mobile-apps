// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
module.exports = {
    // parse an ODBC or ADO.NET connection string into
    parse: function (connectionString) {
        if (!connectionString)
            return {};

        var properties = parseProperties(connectionString),
            server = parseServer(properties['server'] || properties['data source']);

        return connectionString && {
            provider: 'mssql',
            user: properties['user id'] || properties['userid'] || properties['uid'],
            password: properties['password'] || properties['pwd'],
            server: server,
            port: parsePort(properties['server'] || properties['data source'] || properties['datasource']),
            database: properties['database'] || properties['initial catalog'] || properties['initialcatalog'],
            connectionTimeout: (parseInt(properties['connection timeout'] || properties['connectiontimeout']) * 1000) || 15000,
            options: {
                // Azure requires encryption
                encrypt: module.exports.serverRequiresEncryption(server) || parseBoolean(properties['encrypt'])
            }
        };

        function parseServer(value) {
            if(!value)
                return '';

            var start = value.indexOf(':'),
                end = value.lastIndexOf(',');

            return value.substring(start + 1, end === -1 ? undefined : end);
        }

        function parsePort(value) {
            if(!value)
                return 1433;

            var start = value.lastIndexOf(',');
            return start === -1 ? undefined : parseInt(value.substring(start + 1));
        }

        function parseBoolean(value) {
            value = value && value.toLowerCase();
            return !!(value === 'true' || value === 'yes');
        }

        function parseProperties(source) {
            return source.split(';').reduce(function (properties, property) {
                var keyValue = property.split('='),
                    key = keyValue[0].toLowerCase(),
                    value = keyValue.length > 1 && keyValue[1];

                if(key)
                    properties[key] = value;

                return properties;
            }, {});
        }
    },
    serverRequiresEncryption: function (server) {
        return server && (server.indexOf('database.windows.net') > -1
            || server.indexOf('database.usgovcloudapi.net') > -1
            || server.indexOf('database.cloudapi.de') > -1
            || server.indexOf('database.chinacloudapi.cn') > -1);
    }
};
