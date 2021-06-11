// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
The azure-mobile-apps logging framework, configured using {@link loggingConfiguration}
@module azure-mobile-apps/src/logger
*/
var winston = require('winston'),
    logger = new (winston.Logger)({
        // set reasonable defaults, mostly for logging errors that occur loading
        // configuration, before the logger has actually been configured
        level: 'info',
        transports: [new (winston.transports.Console)()]
    });

/**
Exports an instance of a winston logger with the additional members described below.
@see {@link https://github.com/winstonjs/winston}
*/
module.exports = logger;
