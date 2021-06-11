// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers');

module.exports = function (tableConfig) {
		var tableName = helpers.formatTableName(tableConfig);
    return {
        sql: 'EXEC sp_helpindex N\'' + tableName + '\''
    };
};
