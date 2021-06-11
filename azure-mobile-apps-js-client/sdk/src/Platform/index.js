// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var target = require('./environment').getTarget();

if (target === 'Cordova') {
    module.exports = require('./cordova');
} else if (target === 'Web') {
    module.exports = require('./web');
} else {
    throw new Error('Unsupported target');
}
