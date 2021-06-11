// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var Validate = require('../Utilities/Validate'),
    Platform = require('../Platform'),
    constants = require('../constants'),
    _ = require('../Utilities/Extensions');

exports.Push = Push;

/**
 * @class
 * @classdesc Push registration manager.
 * @protected
 */
function Push(client, installationId) {
    this.client = client;
    this.installationId = installationId;
}

/**
 * Register a push channel with the Azure Mobile Apps backend to start receiving notifications.
 *
 * @function
 * 
 * @param {string} platform The device platform being used - _'wns'_, _'gcm'_ or _'apns'_.
 * @param {string} pushChannel The push channel identifier or URI.
 * @param {string} templates An object containing template definitions. Template objects should contain body, headers and tags properties.
 * @param {string} secondaryTiles An object containing template definitions to be used with secondary tiles when using WNS.
 * 
 * @returns {Promise} A promise that is resolved when registration is successful OR rejected with the error if it fails.
 */
Push.prototype.register = Platform.async(
    function (platform, pushChannel, templates, secondaryTiles, callback) {
        Validate.isString(platform, 'platform');
        Validate.notNullOrEmpty(platform, 'platform');

        // in order to support the older callback style completion, we need to check optional parameters
        if (_.isNull(callback) && (typeof templates === 'function')) {
            callback = templates;
            templates = null;
        }

        if (_.isNull(callback) && (typeof secondaryTiles === 'function')) {
            callback = secondaryTiles;
            secondaryTiles = null;
        }

        var requestContent = {
            installationId: this.installationId,
            pushChannel: pushChannel,
            platform: platform,
            templates: stringifyTemplateBodies(templates),
            secondaryTiles: stringifyTemplateBodies(secondaryTiles)
        };

        executeRequest(this.client, 'PUT', pushChannel, requestContent, this.installationId, callback);
    }
);

/**
 * Unregister a push channel with the Azure Mobile Apps backend to stop receiving notifications.
 * 
 * @function
 * 
 * @param {string} pushChannel The push channel identifier or URI.
 * 
 * @returns {Promise} A promise that is resolved if unregister is successful and rejected with the error if it fails.
 */
Push.prototype.unregister = Platform.async(
    function (pushChannel, callback) {
        executeRequest(this.client, 'DELETE', pushChannel, null, this.installationId, callback);
    }
);

function executeRequest(client, method, pushChannel, content, installationId, callback) {
    Validate.isString(pushChannel, 'pushChannel');
    Validate.notNullOrEmpty(pushChannel, 'pushChannel');

    var headers = { 'If-Modified-Since': 'Mon, 27 Mar 1972 00:00:00 GMT' };
    headers[constants.apiVersionHeaderName] = constants.apiVersion;

    client._request(
        method,
        'push/installations/' + encodeURIComponent(installationId),
        content,
        null,
        headers,
        callback
    );
}

function stringifyTemplateBodies(templates) {
    var result = {};
    for (var templateName in templates) {
        if (templates.hasOwnProperty(templateName)) {
            // clone the template so we are not modifying the original
            var template = _.extend({}, templates[templateName]);
            if (typeof template.body !== 'string') {
                template.body = JSON.stringify(template.body);
            }
            result[templateName] = template;
        }
    }
    return result;
}