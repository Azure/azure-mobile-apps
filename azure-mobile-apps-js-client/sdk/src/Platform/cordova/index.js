// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/**
 * Defines Cordova implementation of target independent APIs.
 * For now, the browser implementation works as-is for Cordova, so we 
 * just reuse the browser definitions.
 * @private
 */

var browserExports = require('../web');

// Copy the browser exports into the exports object for Cordova, instead of module.exports = browserExports.
// This way we can add more exports to module.exports (in the future) without having to worry about 
// having an unintended side effect on the browser exports. 
for (var i in browserExports) {
    exports[i] = browserExports[i];
}

