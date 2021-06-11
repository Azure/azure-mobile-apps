// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
@module azure-mobile-apps/src/configuration/DataProviders
@description Azure Mobile Apps ships with two built-in data providers -
Microsoft SQL Server and SQLite3. You can also implement custom data providers
and configure azure-mobile-apps to use them.

## Microsoft SQL Server

The SQL Server provider can be used with any Microsoft SQL Server installation,
including SQL Azure. To use the provider, specify a value of `mssql` for the
`provider` property of data configuration. The provider is also used when deployed
to Azure used if a Data Connection has been configured in the Azure Portal for the app.

The following options can be used with the SQL Server provider:

Property | Description
:--- | :---
server | Hostname of the database server
database | Name of the database to connect to
user | User name to connect with
password | Password for user
port | Port to connect to
connectionTimeout | Connection timeout in milliseconds
connectionString | SQL Server connection string
options | Additional connection options

You must specify either a `connectionString` or all four of `server`, `database`,
`user` and `password`. Options correspond with those from the
[mssql npm package documentation](https://github.com/patriksimek/node-mssql).

The options object can contain the following properties:

Property | Description
:--- | :---
encrypt|Encrypt the connection. Required and turned on automatically for SQL Azure
instanceName|Instance name of the SQL Server to connect to

These options correspond with those from the Tedious options from the
[mssql documentation](https://github.com/patriksimek/node-mssql).

### Examples

#### Full example showing instantiation of an azure-mobile-apps application.
```Javascript
var app = require('express')(),
    mobileApp = require('azure-mobile-apps')({
        data: {
            provider: 'mssql',
            server: 'localhost',
            database: 'myMobileAppDatabase',
            user: 'foo',
            password: 'bar'
        }
    });

mobileApp.tables.add('todoitem');
app.use(mobileApp);
app.listen(process.env.PORT || 3000);
```

#### Separate configuration file with connection to SQL Azure database
This file should be called `azureMobile.js` in the root directory of your app.
```Javascript
module.exports = {
    data: {
        provider: 'mssql',
        server: 'myAzureSqlServer.database.windows.net',
        database: 'myMobileAppDatabase',
        user: 'azureUser',
        password: 'azurePassword'
    }
};
```

#### Configuration using a connection string
```Javascript
module.exports = {
    data: {
        provider: 'mssql',
        connectionString: "Data Source=localhost;Initial Catalog=myMobileAppDatabase;User ID=foo;Password=bar"
    }
};
```

## SQLite3

The SQLite3 data provider that ships with azure-mobile-apps is intended for local
development purposes and not for production scenarios. The provider is intended
to be an embedded data provider that works out of the box with zero configuration.

To use the provider, the sqlite3 module must be installed as a dependency of your app:

    npm i sqlite3

It is recommended to not use the --save option - this will ensure the sqlite3
module is not installed when deployed to Azure - as mentioned, the provider
is not recommended for production use.

Once installed, the provider requires no configuration and is set to be the default
data provider. The only option that can be used is the `filename` option. This
specifies that data should be stored to a persistent file.

### Examples

#### Full example using default configuration
```Javascript
var app = require('express')(),
    mobileApp = require('azure-mobile-apps')();

mobileApp.tables.add('todoitem');
app.use(mobileApp);
app.listen(process.env.PORT || 3000);
```

#### Configuration to a persistent file
This file should be called `azureMobile.js` in the root directory of your app.
```Javascript
module.exports = {
    data: {
        provider: 'sqlite',
        filename: 'mobile.sqlite'
    }
};
```

## Custom Providers

You can implement custom data providers with full or partial functionality
and configure azure-mobile-apps to consume the provider by assigning the
factory function to the provider property of the data configuration object.

For more information on implementing custom data providers, check out the
[contributor guidelines](https://github.com/Azure/azure-mobile-apps-node/blob/master/src/data/contributor.md).

### Example

```Javascript
var app = require('express')(),
    customDataProvider = require('./dataProvider'),
    mobileApp = require('azure-mobile-apps')({
        data: {
            provider: customDataProvider
        }
    });

mobileApp.tables.add('todoitem');
app.use(mobileApp);
app.listen(process.env.PORT || 3000);
```
*/
