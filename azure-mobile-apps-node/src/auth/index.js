// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/auth
@description Helper functions for working with JWT tokens
*/
var user = require('./user'),
    jwt = require('jsonwebtoken'),
    promises = require('../utilities/promises');

/**
Create an instance of a helper based on the supplied configuration.
@param {authConfiguration} configuration The authentication configuration
@returns An object with members described below.
*/
module.exports = function (configuration) {
    var key = configuration.azureSigningKey ? hexStringToBuffer(configuration.azureSigningKey) : configuration.secret;

    return {
        /**
        Validate a JWT token
        @param {string|Buffer} token The JWT token to validate
        @returns A promise that yields a {@link module:azure-mobile-apps/src/auth/user user} object on success.
        @example
var auth = require('azure-mobile-apps/src/auth')(mobileApp.configuration.auth);
if(auth.validate(req.get('x-zumo-auth')))
    res.status(200).send("Successfully authenticated");
else
    res.status(401).send("You must be logged in");
        */
        validate: function (token) {
            return promises.create(function (resolve, reject) {
                var options = {
                    audience: configuration.audience || 'urn:microsoft:windows-azure:zumo',
                    issuer: configuration.issuer || 'urn:microsoft:windows-azure:zumo'
                };

                jwt.verify(token, key, options, function (err, claims) {
                    if(err)
                        reject(err);
                    else
                        resolve(user(configuration, token, claims));
                });
            });
        },
        /**
        Decode a JWT token without validating
        @param {string} token The payload to sign
        @returns {module:azure-mobile-apps/src/auth/user} A user object for the token
        */
        decode: function (token) {
            return user(configuration, token, jwt.decode(token));
        },
        /**
        Create a token from the specified payload
        @param {object} payload The payload to sign
        @returns {string} The signed token.
        @example
var auth = require('azure-mobile-apps/src/auth')(mobileApp.configuration.auth);
res.status(200).send(auth.sign({ sub: "myUserId" }));
        */
        sign: function (payload) {
            var options = { };

            if(!payload.aud)
                options.audience = configuration.audience || 'urn:microsoft:windows-azure:zumo';

            if(!payload.iss)
                options.issuer = configuration.issuer || 'urn:microsoft:windows-azure:zumo';

            if(!payload.exp)
                options.expiresIn = (configuration.expires || 1440) * 60;

            return jwt.sign(payload, key, options);
        }
    };
};

function hexStringToBuffer(hexString) {
    var bytes = [];
    for (var i = 0; i < hexString.length; i += 2)
        bytes.push(parseInt(hexString.substr(i, 2), 16));
    return new Buffer(bytes);
}
