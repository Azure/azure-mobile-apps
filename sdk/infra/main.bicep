targetScope = 'resourceGroup'

// Deployment Instructions
//
// az login
// az account set --subscription <mysub>
// az group create -l <location> -n <name>
// az deployment group create --name d-<name> --resource-group <name> --template-file ./infra/main.bicep

@minLength(1)
@description('Primary location for all resources')
param location string = resourceGroup().location

@description('SQL Server administrator login')
param sqlAdminUsername string = 'testadmin'

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string = newGuid()

param flexibleServerSkuName string = 'Standard_B1ms'
param flexibleServerSkuType string = 'Burstable'
param containerName string = 'Movies'

var resourceToken = toLower(uniqueString(subscription().id, resourceGroup().name, location))

// Azure SQL Server and Database
resource azsql_server 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: 'azsql-${resourceToken}'
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    publicNetworkAccess: 'Enabled'
  }

  resource azsql_azurefw 'firewallRules' = {
    name: 'AllowAllAzureIps'
    properties: {
      endIpAddress: '0.0.0.0'
      startIpAddress: '0.0.0.0'
    }
  }

  resource azsql_firewall 'firewallRules' = {
    name: 'AllowPublicAccess'
    properties: {
      endIpAddress: '255.255.255.255'
      startIpAddress: '0.0.0.0'
    }
  }
}

resource azsql_database 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  name: 'unittests'
  location: location
  parent: azsql_server
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 104857600
  }
}

// PostgreSQL Server and Database
resource pgsql_server 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: 'pgserver-${resourceToken}'
  location: location
  sku: {
    name: flexibleServerSkuName
    tier: flexibleServerSkuType
  }
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    createMode: 'Default'
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
      autoGrow: 'Disabled'
    }
    version: '15'
  }

  resource pgsql_azurefw 'firewallRules' = {
    name: 'AllowAllAzureIps'
    properties: {
      endIpAddress: '0.0.0.0'
      startIpAddress: '0.0.0.0'
    }
  }

  resource pgsql_firewall 'firewallRules' = {
    name: 'AllowPublicAccess'
    properties: {
      endIpAddress: '255.255.255.255'
      startIpAddress: '0.0.0.0'
    }
  }
}

resource pgsql_database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  name: 'unittests'
  parent: pgsql_server
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Cosmos SQL Account, Database, and Container
resource cosmos_account 'Microsoft.DocumentDB/databaseAccounts@2023-09-15' = {
  name: 'cosmos-${resourceToken}'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
  }
}

resource cosmos_database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-09-15' = {
  name: 'unittests'
  parent: cosmos_account
  properties: {
    resource: {
      id: 'unittests'
    }
  }
}

resource cosmos_container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  name: containerName
  parent: cosmos_database
  properties: {
    resource: {
      id: containerName
      partitionKey: {
        paths: [ '/id' ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          { path: '/*' }
        ]
        excludedPaths: [
          { path: '/_etag/?' }
        ]
        compositeIndexes: [
          [
            { path: '/UpdatedAt', order: 'ascending' }
            { path: '/Id', order: 'ascending' }
          ]
        ]
      }
    }
  }
}

// ----------------------------------------------------------------------------------------------------------
//  OUTPUTS
// ----------------------------------------------------------------------------------------------------------

// These should become environment variables for the unit tests
#disable-next-line outputs-should-not-contain-secrets // I'm ok with secrets being output for this deployment
output ZUMO_COSMOS_CONNECTIONSTRING string = cosmos_account.listConnectionStrings().connectionStrings[0].connectionString

#disable-next-line outputs-should-not-contain-secrets // I'm ok with secrets being output for this deployment
output ZUMO_AZSQL_CONNECTIONSTRING string = 'Data Source=tcp:${azsql_server.properties.fullyQualifiedDomainName},1433;Initial Catalog=${azsql_database.name};User Id=${sqlAdminUsername}@${azsql_server.properties.fullyQualifiedDomainName};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False'

#disable-next-line outputs-should-not-contain-secrets // I'm ok with secrets being output for this deployment
output ZUMO_PGSQL_CONNECTIONSTRING string = 'Host=${pgsql_server.properties.fullyQualifiedDomainName};Database=${pgsql_database.name};Username=${sqlAdminUsername};Password=${sqlAdminPassword}'
