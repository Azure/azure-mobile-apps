// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
The top level configuration object.
@typedef configuration
@property {string} platform=express - Server platform to use. Currently only express is supported
@property {string} basePath=. - Base path to use for the application
@property {string} configFile=azureMobile.js - Name of the file that exports configuration objects to load
@property {Promise} promiseConstructor Promise constructor to use
@property {bool} homePage Show a welcome page when navigating to the root mobile app path
@property {bool} swagger Expose swagger metadata at the path set in the swaggerPath configuration setting
@property {bool} skipVersionCheck Allow requests for any API version, i.e. without a zumo-api-version header or querystring parameter
@property {string} apiRootPath=/api - Directory to load api configuration from
@property {string} tableRootPath=/tables - Directory to load table configuration from
@property {string} notificationRootPath=/push/installations - Notification installations endpoint
@property {string} swaggerPath=/swagger - Swagger metadata endpoint
@property {string} authStubRoute=.auth/login/:provider - Route to use to emulate authentication login requests
@property {bool} debug=false - Run the server in debug mode. Automatically turned on when node is executed with the --debug option
@property {string} version - Current version of the Azure Mobile Apps SDK
@property {integer} maxTop=1000 - Limit the maximum number of rows a client can request
@property {integer} pageSize=50 - Set the number of rows for server side paging
@property {webhook} webhook - Webhook options for tables with webhooks enabled
@property {loggingConfiguration} logging - Logging configuration
@property {dataConfiguration} data - Data configuration
@property {authConfiguration} auth - Authentication configuration
@property {corsConfiguration} cors - Cross-origin resource sharing configuration
@property {notificationsConfiguration} notifications - Notifications configuration
@property {storageConfiguration} storage - Storage account configuration
*/

/**
Logging configuration. See {@link https://github.com/winstonjs/winston#instantiating-your-own-logger}
@typedef loggingConfiguration
@property {string} level=info - Minimum level of messages to log
@property {transports[]} transports=Console - Array of winston transports to log messages to
*/

/**
Data configuration.
@typedef dataConfiguration
@property {string|function} provider=sqlite - Data provider to use. Supported providers are mssql and sqlite. 
You can also pass a custom data provider factory function here. See 
[the contributor guidelines](https://github.com/Azure/azure-mobile-apps-node/blob/master/src/data/contributor.md) for more information.
@property {bool} dynamicSchema=false - Global default for table dynamic schema, can override at table config level
@see {@link module:configuration/DataProviders} for more details and examples
@see {@link sqlServerDataConfiguration} for SQL Server specific configuration options
@see {@link sqliteDataConfiguration} for SQLite specific configuration options
*/

/**
SQL Server data configuration.  Can specify a connection with user, password, server and database or a connectionString
@typedef sqlServerDataConfiguration
@property {string} user - User name to connect with
@property {string} password - Password for user
@property {string} server - Hostname of the database server
@property {integer} port=1433 - Port to connect to
@property {string} database - Name of the database to connect to
@property {integer} connectionTimeout=15000 - Connection timeout in milliseconds
@property {string} connectionString - SQL Server connection string
@property {string} schema=dbo - Global default for SQL Server schema name, can override at table config level
@property {Object} options - Additional options
@property {bool} options.encrypt - Encrypt the connection. Required and turned on automatically for SQL Azure
@see {@link https://www.npmjs.com/package/mssql}
*/

/**
SQLite data configuration.
@typedef sqliteDataConfiguration
@property {string} filename - The file name to use to persist data
*/

/**
Authentication configuration
@typedef authConfiguration
@property {string} secret - Key to use to sign and validate JWT tokens
@property {string} azureSigningKey - Key to use to sign and validate JWT tokens, as taken from a hosted web apps WEBSITE_AUTH_SIGNING_KEY environment variable
@property {bool} validateTokens - If the Azure Web App authentication is enabled, JWT tokens are only decoded as validation is already performed
@property {string} audience=urn:microsoft:windows-azure:zumo - Token audience claim
@property {string} issuer=urn:microsoft:windows-azure:zumo - Token issuer claim
@property {integer} expiresInMinutes=1440 - Expiry of signed tokens
@see {@link http://jwt.io/}
@see {@link https://github.com/auth0/node-jsonwebtoken}
*/

/**
Cross-origin resource sharing configuration
@typedef corsConfiguration
@property {string} exposeHeaders=Link,Etag - Response headers to be exposed to the client for CORS requests
@property {integer} maxAge=300 - How long the results of a preflight request can be cached in a preflight result cache,
@property {string[]} hostnames=localhost - Array of allowed hostnames (ignores port and protocol)
*/

/**
Notifications configuration. hubName must be specified. Either a connection string or endpoint and shared access key details mst be provided.
@typedef notificationsConfiguration
@property {string} hubName - The name of the associated notification hub
@property {string} connectionString - The connection string of the associated notification hub
@property {string} endpoint - The name of the endpoint
@property {string} sharedAccessKeyName - Name of the shared access key
@property {string} sharedAccessKeyValue - Shared access key value
@see {@link https://github.com/Azure/azure-sdk-for-node/blob/master/lib/services/serviceBus/lib/notificationhubservice.js}
*/

/**
Storage configuration. Either a connection string or an account and key must be provided.
@typedef storageConfiguration
@property {string} connectionString - The connection string for an Azure storage account
@property {string} account - The name of an Azure storage account
@property {string} key - The access key for the named Azure storage account
*/
