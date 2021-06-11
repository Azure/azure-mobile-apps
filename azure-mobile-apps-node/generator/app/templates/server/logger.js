'use strict';

var winston = require('winston');

var configuration = {
    transports: [
        new winston.transports.Console({
            colorize: true,
            timestamp: true
        })
    ]
};

module.exports = exports = new winston.Logger(configuration);
