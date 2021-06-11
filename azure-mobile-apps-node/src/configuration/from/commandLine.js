// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
Azure Mobile Apps can be configured by providing command line arguments.
Arguments should be prefixed with three dashes (`---`) with the value
in the next agrument. Command line arguments override all other options.
@module azure-mobile-apps/src/configuration/Command Line
@param {string} logging.level Sets the minimum level for log statements to be logged. Valid values are 'error', 'warn', 'info', 'verbose', 'debug' and 'silly'.
@param {string} promiseConstructor=native Sets the promise library being used. Valid vallues are 'q' and 'native'.
*/
var winston = require('winston'),
    q;

try {
    q = require('q');
} catch (ex) {
    // q will be undefined if q is not installed
}

// if this gets much more complex, we should change to using something like optimist
// this method modifies the original configuration parameter
module.exports = function (configuration, commandLineArguments) {
    var args = commandLineArguments || process.argv.slice(2),
        customArgs = {};

    // filter for custom arguments
    args.forEach(function (arg, index) {
        if (arg.slice(0, 3) === '---') {
            customArgs[arg.slice(3)] = args[index + 1];
        }
    });

    Object.keys(customArgs).forEach(function (property) {
        switch (property) {
            case 'logging.level':
                configuration.logging = {
                    level: customArgs[property],
                    transports: [
                        new (winston.transports.Console)({
                            colorize: true,
                            timestamp: true,
                            showLevel: true
                        })
                    ]
                };
                break;

            case 'promiseConstructor':
                configuration.promiseConstructor = ({
                    // this will throw if q is not installed
                    'q': q.Promise,
                    'native': Promise
                })[customArgs[property]];
                break;
        }
    });

    return configuration;
}
