// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

module.exports = function (req, res, next) {
    var context = req.azureMobile;

    // Skip api key validation if a authenticated user exists
    // and authenticated users do not require api keys
    if (context.user && context.configuration.allowUsersWithoutApiKey)
        return next();

    // Validate zumo-api-key header against environment variable.
    // The header could also be validated against config setting, etc
    var apiKey = process.env['zumo-api-key'];
    if (apiKey && req.get('zumo-api-key') != apiKey)
        return res.status(401).send('This operation requires a valid api key');
    else
        return next();
}
