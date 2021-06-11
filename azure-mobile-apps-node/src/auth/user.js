// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/auth/user
@description Encapsulates functionality for authenticated users
*/
var defaultGetIdentity = require('./getIdentity');

/**
Create a new user object.
@param {authConfiguration} authConfiguration The authentication configuration
@param {string} token The JWT token
@param {object} claims The claims associated with the user
@returns An object with the members described below.
*/
module.exports = function (authConfiguration, token, claims) {
    return {
        /** The user ID */
        id: claims.sub,
        /** The JWT token */
        token: token,
        /** The authenticated claims */
        claims: claims,
        /**
        Get associated provider identity information
        @function
        @param {string} provider The name of the authentication provider
        @returns A promise that yields the identity information on success
        */
        getIdentity: getIdentity
    };

    function getIdentity(provider) {
        // if we've been passed a custom getIdentity function, use it. Mainly to support testing without hitting a gateway.
        if(authConfiguration.getIdentity)
            return authConfiguration.getIdentity(authConfiguration, token, provider);
        return defaultGetIdentity(authConfiguration, token, provider);
    }
}
