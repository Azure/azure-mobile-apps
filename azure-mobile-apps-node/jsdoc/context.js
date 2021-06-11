// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@typedef context
@description This object is attached to the express request object as the azureMobile property.
@property {module:queryjs/Query} query The parsed OData query if provided and the parseQuery middleware has been executed
@property {string|number} id The ID associated with the request if provided and the parseQuery middleware has been executed
@property {object} item The item being inserted or updated if provided and the parseItem middleware has been executed
@property {express.Request} req The current express {@link http://expressjs.com/4x/api.html#req request object}
@property {express.Response} res The current express {@link http://expressjs.com/4x/api.html#res response object}
@property {module:azure-mobile-apps/src/data} data The configured data provider
@property {NotificationHubService} [push] The {@link http://azure.github.io/azure-sdk-for-node/azure-sb/latest/NotificationHubService.html|Notification Hubs Service}, if configured
@property {configuration} configuration The azure mobile apps configuration object
@property {module:azure-mobile-apps/src/logger} logger The azure mobile apps logger
@property {function} tables A function that accepts a string table name and returns a table access object for the above provider
@property {module:azure-mobile-apps/src/express/tables/table} table The table definition object for the current table
@property {module:azure-mobile-apps/src/auth/user} user The authenticated user object if the authenticate middleware has been executed
@property {function} execute A function that executes the operation (read, insert, etc) against the table. Returns a promise with the results of the operation
@property {function} next Signal that execution of the request should continue. You can pass an error to this function. Only available in table operation functions.
*/
