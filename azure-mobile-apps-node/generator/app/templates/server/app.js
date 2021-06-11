'use strict';

var azureMobileApp = require('azure-mobile-apps'),
    bodyParser = require('body-parser'),
    compression = require('compression'),
    express = require('express'),
    logCollector = require('express-winston'),
    staticFiles = require('serve-static'),
    logger = require('./logger');

/**
 * Create a new Web Application
 * @param {boolean} [logging=true] - if true, enable transaction logging
 * @returns {Promise.<express.Application>} A promisified express application
 */
function createWebApplication(logging) {
    var app = express(),
        mobile = azureMobileApp();

    if (typeof logging === 'undefined' || logging === true) {
        app.use(logCollector.logger({
            winstonInstance: logger,
            colorStatus: true,
            statusLevels: true
        }));
    }

    app.use(compression());
    app.use(bodyParser.urlencoded({ extended: true }));
    app.use(bodyParser.json());

    app.use(staticFiles('public', {
        dotfile: 'ignore',
        etag: true,
        index: 'index.html',
        lastModified: true
    }));

    mobile.tables.import('./tables');
    mobile.api.import('./api');

    return mobile.tables.initialize()
        .then(function () {
            app.use(mobile);
            app.use(logCollector.errorLogger({
                winstonInstance: logger
            }));
            return app;
        });
}

module.exports = exports = createWebApplication;
