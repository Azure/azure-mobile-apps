// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var express = require('express'),
    azureMobileApps = require('azure-mobile-apps'),

    // Obtain our custom API - it exports an express Router
    categoryApi = require('./api/category');

// Create a standard Express app
var app = express();

// Create a mobile app with default configuration
var mobile = azureMobileApps();

// Add a single table with dynamic schema that is disabled through the REST API
mobile.tables.add('items', { access: 'disabled' });

// Register the Azure Mobile Apps middleware
app.use(mobile);

// Register the router we configure in our custom API module
// This must be done after registering the mobile app
app.use('/api/items', categoryApi(mobile.configuration));

// Listen for requests
app.listen(process.env.PORT || 3000);
