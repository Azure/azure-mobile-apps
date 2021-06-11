// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/*
** Sample Table Definition - this supports the Azure Mobile Apps
** TodoItem product with authentication and offline sync
*/
var azureMobileApps = require('azure-mobile-apps');

// Create a new table definition
var table = azureMobileApps.table();

// Configure specific code when the client does a request
// READ - only return records belonging to the authenticated user
table.read(function (context) {
  context.query.where({ userId: context.user.id });
  return context.execute();
});

// CREATE - add or overwrite the userId based on the authenticated user
table.insert(function (context) {
  context.item.userId = context.user.id;
  return context.execute();
});

// UPDATE - for this scenario, we don't need to do anything - this is
// the default version
table.update(function (context) {
 return context.execute();
});

// DELETE - for this scenario, we don't need to do anything - this is
// the default version
table.delete(function (context) {
 return context.execute();
});

// Finally, export the table to the Azure Mobile Apps SDK - it can be
// read using the azureMobileApps.tables.import(path) method

module.exports = table;
