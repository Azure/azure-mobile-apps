// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var sqlite3, connections = {};

try {
    sqlite3 = require('sqlite3');
} catch(ex) {
    throw new Error('To use the sqlite data provider, you must install the sqlite3 module by running "npm i sqlite3"');
}

module.exports = function (configuration) {
    var filename = (configuration && configuration.filename) || ':memory:';

    if(!connections[filename])
        connections[filename] = new sqlite3.cached.Database(filename);

    return connections[filename];
};
