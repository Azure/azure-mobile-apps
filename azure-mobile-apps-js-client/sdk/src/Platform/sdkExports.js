// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var target = require('./environment').getTarget();

if (target === 'Cordova') {
    module.exports = require('./cordova/sdkExports');
} else if (target === 'Web') {
    module.exports = require('./web/sdkExports');
} else {
    throw new Error('Unsupported target');
}
