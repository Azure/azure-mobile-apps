// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
// A simple request module - currently only used from webhooks, avoiding adding another dependency
var log = require('../logger'),
    http = require('http'),
    https = require('https'),
    parseUrl = require('url').parse;

module.exports = function (url, payload) {
    var parsed = parseUrl(url),
        client = parsed.protocol === 'https' ? https : http,
        serialized = JSON.stringify(payload),
        options = {
            hostname: parsed.hostname,
            port: parsed.port,
            path: parsed.path,
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Content-Length': serialized.length
            }
        };
    
    var req = client.request(options, function (response) {
        var body = '';

        response.on('data', function (chunk) {
            body += chunk;
        });

        response.on('end', function () {
            if(response.statusCode !== 200)
                log.error("Error returned from request: (" + response.statusCode + ") " + body);
        });
    });

    req.on('error', function (err) {
        log.error("Unable to connect to server:", err);        
    });

    req.write(serialized);
    req.end();
};
