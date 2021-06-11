// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var fs = require('fs'),
    path = require('path'),
    util = require('util'),

    cache = {};

module.exports = function (name, args) {
    if(!cache[name])
        cache[name] = fs.readFileSync(path.resolve(__dirname, name)).toString();
    return util.format.apply(null, [cache[name]].concat(Array.prototype.slice.call(arguments, 1)));
};
