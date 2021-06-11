// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/*
** Sample Table Definition - this supports the Azure Mobile Apps
** TodoItem product
*/
var azureMobileApps = require('azure-mobile-apps'),
    validateApiKey = require('../validateApiKey');

// Create a new table definition
var table = azureMobileApps.table();

// Access should be anonymous so that unauthenticated users are not rejected
// before our custom validateApiKey middleware runs.
table.access = 'anonymous';

// validate api key header prior to execution of any table operation
table.use(validateApiKey, table.execute);
// to require api key authentication for only one operation (in this case insert)
// instead of table.use(validateApiKey, table.execute) use:
// table.insert.use(validateApiKey, table.operation);

module.exports = table;
