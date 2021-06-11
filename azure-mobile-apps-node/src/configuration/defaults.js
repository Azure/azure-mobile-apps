// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var winston = require('winston'),
    path = require('path'),
    environment = require('../utilities/environment'),
    packageJson = require('../../package.json');

module.exports = function () {
    return {
        platform: 'express',
        basePath: basePath(),
        configFile: 'azureMobile',
        promiseConstructor: Promise,
        apiRootPath: '/api',
        tableRootPath: '/tables',
        notificationRootPath: '/push/installations',
        swaggerPath: '/swagger',
        authStubRoute: '/.auth/login/:provider',
        debug: environment.debug,
        version: 'node-' + packageJson.version,
        apiVersion: '2.0.0',
        homePage: false,
        swagger: false,
        maxTop: 1000,
        pageSize: 50,
        userIdColumn: 'userId',
        logging: {
            level: environment.debug ? 'debug' : 'info',
            transports: [
                new (winston.transports.Console)({
                    colorize: true,
                    timestamp: true,
                    showLevel: true
                })
            ]
        },
        cors: {
            exposeHeaders: 'Link,Etag',
            maxAge: 300,
            hostnames: ['localhost']
        },
        data: {
            provider: 'sqlite',
            schema: 'dbo',
            dynamicSchema: true
        },
        auth: {
            secret: '0000'
        },
        notifications: { },
        storage: { }
    };
};

function basePath() {
    return environment.hosted
        ? (process.env.HOME || '') + "\\site\\wwwroot"
        : path.dirname(require.main.filename);
}
