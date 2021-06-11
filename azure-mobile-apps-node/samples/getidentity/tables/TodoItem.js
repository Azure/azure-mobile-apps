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

table.columns = {
 "emailAddress": "string",
 "text": "string",
 "complete": "boolean"
};
table.dynamicSchema = false;
table.access = 'authenticated';

// Configure specific code when the client does a request
// READ - only return records belonging to the authenticated user
table.read(function (context) {
    return context.user.getIdentity().then((data) => {
        context.query.where({ emailAddress: data.microsoftaccount.claims.emailaddress });
        return context.execute();
    });
});

// CREATE - add or overwrite the userId based on the authenticated user
table.insert(function (context) {
    return context.user.getIdentity().then((data) => {
        context.item.emailAddress = data.microsoftaccount.claims.emailaddress;
        return context.execute();
    });
});

// UPDATE - only allow updating of record belong to the authenticated user
table.update(function (context) {
    return context.user.getIdentity().then((data) => {
        context.query.where({ emailAddress: data.microsoftaccount.claims.emailaddress });
        return context.execute();
    });
});

// DELETE - only allow deletion of records belong to the authenticated uer
table.delete(function (context) {
    return context.user.getIdentity().then((data) => {
        context.query.where({ emailAddress: data.microsoftaccount.claims.emailaddress });
        return context.execute();
    });
});

module.exports = table;
