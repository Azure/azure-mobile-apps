// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/utilities/environment
@description Provides utility functions for determining aspects of the environment.
*/
module.exports = {
    /** A property indicating if Node.js was started in debug mode */
    debug: process.execArgv.some(function (arg) {
        return arg.indexOf('--debug') === 0;
    }),
    /** A property indicating if the app is hosted on an Azure instance */
    hosted: !!process.env.WEBSITE_HOSTNAME
};
