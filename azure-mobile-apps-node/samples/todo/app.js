// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// This is a base-level Azure Mobile App SDK.
var express = require('express'),
    azureMobileApps = require('azure-mobile-apps');

// Set up a standard Express app
var app = express();

// If you are producing a combined Web + Mobile app, then you should handle
// anything like logging, registering middleware, etc. here

// Configuration of the Azure Mobile Apps can be done via an object, the
// environment or an auxiliary file.  You can check out the default object
// within node_modules/azure-mobile-apps/index.js (look for defaults).  You
// can also export an object using azureMobile.js.  In this sample, check
// out the example azureMobile.js file
var mobile = azureMobileApps({
    // Explicitly enable the Azure Mobile Apps home page
    homePage: true,
    // Explicitly enable the Swagger UI - swagger endpoint is at /swagger
    // and the UI is at /swagger/ui
    swagger: true
});

// Import the files from the tables directory to configure the /tables API
mobile.tables.import('./tables');

// Initialize the database before listening for incoming requests
// The tables.initialize() method does the initialization asynchronously
// and returns a Promise.
mobile.tables.initialize()
    .then(function () {
        app.use(mobile);    // Register the Azure Mobile Apps middleware
        app.listen(process.env.PORT || 3000);   // Listen for requests
    });
