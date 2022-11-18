@description('The name of the Azure Cosmos DB account')
param accountName string

@description('The name for the Azure Cosmos DB database')
param databaseName string 

@description('The name for the Azure Cosmos DB container')
param containerName string

@description('The throughput for the Azure Cosmos DB container')
@minValue(400)
param throughput int = 400

@description('If true, enable the free tier')
param enableFreeTier bool = false

@description('The default consistency level for the account')
@allowed([
  'Eventual'
  'ConsistentPrefix'
  'Session'
  'Strong'
])
param consistencyLevel string = 'Session'

@description('Location for all resources')
param location string = resourceGroup().location

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' = {
  name: toLower(accountName)
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: consistencyLevel
    }
    enableFreeTier: enableFreeTier
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  name: databaseName
  parent: cosmosAccount
  properties: {
    resource: {
      id: databaseName
    }
    options: {
      throughput: throughput
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-08-15' = {
  name: containerName
  parent: cosmosDatabase
  properties: {
    resource: {
      id: containerName
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          { 
            path: '/*'
          }
        ] 
        excludedPaths: [
          {
            path: '/_etag/?'
          }
        ]
      }
    }
  }
}

#disable-next-line outputs-should-not-contain-secrets
output connectionString string = cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
