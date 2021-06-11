// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
module.exports = function (table) {
    return {
        sql: 'TRUNCATE TABLE ' + table.name
    }
}
