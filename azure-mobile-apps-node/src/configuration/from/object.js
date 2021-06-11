// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var merge = require('../../utilities/assign');

module.exports = function (configuration, source) {
    return merge(configuration, source);
};
