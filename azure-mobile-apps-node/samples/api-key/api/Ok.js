// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var validateApiKey = require('../validateApiKey');

// validate api key header prior to api method execution
module.exports = {
    // api methods can be defined with an array of middleware functions
    get: [ validateApiKey, function (req, res, next) {
        res.status(200).send();
    }]
}