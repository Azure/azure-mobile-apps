targetScope = 'resourceGroup'

param environmentName string
param location string = resourceGroup().location
//param principalId string
param resourceToken string = toLower(uniqueString(subscription().id, environmentName, location))
param tags object = {}

param cosmosDatabaseName string = 'tododb'

// Cosmos Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: 'cosmos-${resourceToken}'
  kind: 'GlobalDocumentDB'
  location: location
  tags: tags
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Strong'
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

// Cosmos Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  name: cosmosDatabaseName
  parent: cosmosAccount
  tags: tags
  properties: {
    resource: {
      id: cosmosDatabaseName
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  name: 'TodoItems'
  parent: cosmosDatabase
  properties: {
    resource: {
      id: 'TodoItems'
      partitionKey: {
        paths: [
          '/Id'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
        compositeIndexes: [
          [
            {
              path: '/UpdatedAt'
              order: 'ascending'
            }
            {
              path: '/Id'
              order: 'ascending'
            }
          ]
        ]
      }
    }
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'asp-${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'F1'
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: 'api-${resourceToken}'
  location: location
  tags: union({ 'azd-service-name': 'api' }, tags)
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'Disabled'
    }
  }

  resource appSettings 'config' = {
    name: 'appsettings'
    properties: {
      ASPNETCORE_ENVIRONMENT: 'Development'
      AZURE_COSMOS_ENDPOINT: cosmosAccount.properties.documentEndpoint
      AZURE_COSMOS_DATABASE: cosmosDatabase.name
      AZURE_COSMOS_CONNECTION_STRING: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
    }
  }
}
