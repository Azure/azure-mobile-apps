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

// Configure the table schema - there are two options:
//  1) A Static schema

//table.columns = {
//  "userId": "string",
//  "text": "string",
//  "complete": "boolean"
//};

//  2) A Dynamic schema
table.dynamicSchema = true;

// Configure table options
table.access = 'authenticated';

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

// UPDATE - only allow updating of record belong to the authenticated user
table.update(function (context) {
    context.query.where({ userId: context.user.id });
    return context.execute();
});

// DELETE - only allow deletion of records belong to the authenticated uer
table.delete(function (context) {
    context.query.where({ userId: context.user.id });
    return context.execute();
});
// An example to disable deletions - the same operation can be used on
// any table operation (read, insert, update, delete)
//table.delete.access = 'disabled';

// Finally, export the table to the Azure Mobile Apps SDK - it can be
// read using the azureMobileApps.tables.import(path) method
module.exports = table;
