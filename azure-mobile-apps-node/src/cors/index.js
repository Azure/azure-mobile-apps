// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/cors
@description Helper functions for negotiating cross-origin resource sharing requests
*/
var url = require('url');

/**
Create an instance of a helper based on the supplied configuration.
@param {corsConfiguration} configuration The CORS configuration
@returns An object with members described below.
*/
module.exports = function (configuration) {
    var isNullAllowed = false,
        headersRegex = /^[a-z0-9\-\,\s]{1,500}$/i,
        originRegexes = buildOriginRegexes(configuration && configuration.hostnames);

    var api = {
        /**
        Get appropriate CORS response headers for the supplied options
        @function getHeaders
        @param {string} origin The origin of the request
        @param {string} headers A comma delimited list of allowed headers
        @param {string} method The HTTP method of the request
        @returns An object containing the appropriate response headers
        */
        getHeaders: function(origin, headers, method) {
            var responseHeaders = {};
            if (origin && api.isAllowedOrigin(origin)) {
                // CORS doesn't permit multiple origins or wildcards, so the standard
                // pattern is to validate the incoming origin and echo it back if accepted.
                responseHeaders['Access-Control-Allow-Origin'] = origin;
                responseHeaders['Access-Control-Expose-Headers'] = configuration.exposeHeaders;

                if (headers && isAllowedHeaders(headers)) {
                    // CORS doesn't permit * here, so echo back whatever is requested
                    // assuming it doesn't contain bad characters and isn't too long.
                    responseHeaders['Access-Control-Allow-Headers'] = headers;
                }

                if (method === 'OPTIONS') {
                    // we only want to send these headers on preflight requests
                    responseHeaders['Access-Control-Allow-Methods'] = 'GET, PUT, PATCH, POST, DELETE, OPTIONS';
                    responseHeaders['Access-Control-Max-Age'] = configuration.maxAge || 300;
                }
            }
            return responseHeaders;
        },

        /**
        Determines if the origin is allowed according to the supplied configuration
        @function isAllowedOrigin
        @param {string} origin The origin to verify
        @returns {boolean}
        */
        isAllowedOrigin: function(origin) {
            // special case 'null' that is sent from browser on local files
            if (isNullAllowed && origin === 'null') {
                return true;
            }

            var parsedOrigin = url.parse(origin),
                originHostName = parsedOrigin && parsedOrigin.hostname,
                originProtocol = parsedOrigin && parsedOrigin.protocol,
                originPath = parsedOrigin && parsedOrigin.path;

            // Validate protocol
            if (!originProtocol || !isAllowedProtocol(originProtocol)) {
                return false;
            }

            // Validate path (note: it's typically null)
            if (!isAllowedPath(originPath)) {
                return false;
            }

            // Validate host name
            if (!originHostName) {
                return false;
            }

            return originRegexes.some(function(originRegex) {
                return originRegex.test(originHostName);
            });
        }
    };

    return api;

    function isAllowedHeaders(headers) {
        return headersRegex.test(headers);
    }

    function isAllowedProtocol(protocol) {
        // This means that filesystem origins ("null") aren't supported right now
        // even if you allow "*"
        return protocol === 'http:' || protocol === 'https:' || protocol === 'ms-appx-web:';
    }

    function isAllowedPath(path) {
        // The W3C spec isn't especially clear about host origins should be formatted,
        // so to be graceful we permit trailing slashes even though I'm not aware of a
        // browser that sends them. But for the sake of being locked down, anything
        // beyond the slash is disallowed.
        return !path || path === '/';
    }

    function parseOrigin(origin) {
        // included for compatibility with V1 { 'host': 'www.example.com' } origins
        if (typeof(origin) === 'object') {
            origin = origin.host;
        } else {
            try {
                origin = JSON.parse(origin);
                origin = origin.host;
            } catch (e) {}
        }
        return origin;
    }

    function buildOriginRegexes(hostnames) {
        if(!hostnames)
            return [];

        return hostnames.map(function(origin) {
            // included for compatibility with V1 { 'host': 'www.example.com' } origins
            origin = parseOrigin(origin);

            if (origin === 'null') {
                isNullAllowed = true;
            }

            // escape all special characters
            origin = origin.replace(/([.?*+\^$\[\]\\(){}|\-])/g, '\\$1');

            // support wildcard matching
            origin = origin.replace(/\\\*/, '[a-z0-9\\-\\.]*');

            return new RegExp('^' + origin + '$', 'i');
        });
    }
};
