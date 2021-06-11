// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var convert = require('../convert');

module.exports = {
    prepareItems: function(table) {
        return function (rows) {
            rows = rows.map(function (row) {
                return convert.item(table.sqliteColumns, row);
            });

            if (rows.length === 0)
                return undefined;
            else if (rows.length === 1)
                return rows[0];
            else
                return rows;
        };
    },
    ignoreResults: function () {}
}
