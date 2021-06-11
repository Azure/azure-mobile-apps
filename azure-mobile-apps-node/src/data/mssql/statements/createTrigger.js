// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var helpers = require('../helpers');

module.exports = function (tableConfig) {
    var tableName = helpers.formatTableName(tableConfig);
    return {
        sql: 'CREATE TRIGGER [TR_' + tableConfig.name + '_InsertUpdateDelete] ON ' + tableName + ' AFTER INSERT, UPDATE, DELETE AS BEGIN SET NOCOUNT ON; IF TRIGGER_NESTLEVEL() > 3 RETURN; UPDATE ' + tableName + ' SET [updatedAt] = CONVERT (DATETIMEOFFSET(7), SYSUTCDATETIME()) FROM INSERTED WHERE INSERTED.id = ' + tableName + '.[id] END'
    }
}
