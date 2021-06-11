// ----------------------------------------------------------------------------
// Copyright (c) 2016 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/*
** Sample Table Definition - this supports the Azure Mobile Apps
** TodoItem product with authentication and offline sync
*/
import azureMobileApps from 'azure-mobile-apps';

// Create a new table definition
let table = azureMobileApps.table();

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
table.read((context) => {
    context.query.where({ userId: context.user.id });
    return context.execute();
});

// CREATE - add or overwrite the userId based on the authenticated user
table.insert((context) => {
    context.item.userId = context.user.id;
    return context.execute();
});

// UPDATE - for this scenario, we don't need to do anything - this is
// the default version
//table.update((context) => {
//  return context.execute();
//});

// DELETE - for this scenario, we don't need to do anything - this is
// the default version
//table.delete((context) => {
//  return context.execute();
//});
// An example to disable deletions
//table.delete.access = 'disabled';

// Finally, export the table to the Azure Mobile Apps SDK - it can be
// read using the azureMobileApps.tables.import(path) method
export default table;
