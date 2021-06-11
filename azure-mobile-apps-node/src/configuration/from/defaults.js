// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var defaults = require('../defaults'),
    merge = require('../../utilities/assign');

module.exports = function (configuration, overrides) {
    return merge(configuration || {}, defaults(), overrides);
};
