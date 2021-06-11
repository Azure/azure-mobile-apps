// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var table = require('azure-mobile-apps').table();

table.access = 'authenticated';
table.columns = {
    "userId": "string",
    "tag": "string"
};

table.insert(function (context) {
    // we can perform validation here, e.g. that the user is allowed to register for the tag
    context.item.userId = context.user.id;
    return context.execute();
});

module.exports = table;
