// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = function (claims) {
    if(claims.constructor !== Array)
        claims = [claims];

    return claims.reduce(function (target, identity) {
        identity.claims = identity.user_claims.reduce(mapClaims, {});
        target[identity['provider_name']] = identity;
        return target;
    }, {});

    function mapClaims(target, claim) {
        setDefaults(target, claim.typ, claim.val);
        setClaim(target, claim.typ, claim.val);
        denormalizeClaim(target, claim, 'http://schemas.xmlsoap.org/ws', '/');
        denormalizeClaim(target, claim, 'http://schemas.microsoft.com', '/');
        denormalizeClaim(target, claim, 'urn:', ':');
        return target;
    }

    function setDefaults(target, type, value) {
        if(type === 'groups') {
            if(target[type] === undefined)
                target[type] = [];
        }
    }

    function setClaim(target, type, value) {
        var existingValue = target[type];
        if(existingValue === undefined || existingValue === null)
            target[type] = value;
        else if (existingValue.constructor === Array)
            existingValue.push(value);
        else
            target[type] = [existingValue, value];
    }

    function denormalizeClaim(target, claim, schema, separator) {
        if (claim.typ.indexOf(schema) === 0)
            setClaim(target, claim.typ.slice(claim.typ.lastIndexOf(separator) + 1), claim.val);
    }
};
