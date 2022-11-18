@description('Default location for all resources')
param location string = resourceGroup().location

@description('The name of the Cosmos DB service account')
param cosmosAccountName string = 'cosmos${uniqueString(resourceGroup().id)}'

@description('The name of the default Cosmos DB database')
param cosmosDatabaseName string = 'todoitems'

@description('The name of the Cosmos DB container')
param cosmosContainerName string = 'default'

@description('The configured throughput for the Cosmos DB container')
param cosmosThroughput int = 400

@description('The name of the App Service')
param appServiceName string = 'app${uniqueString(resourceGroup().id)}'

@description('The name of the App Service Hosting Plan')
param appServiceHostingPlanName string = 'hosting${uniqueString(resourceGroup().id)}'

@description('App Service Hosting Plan SKU')
param appServiceSku string = 'F1'

module cosmosDb './bicep-modules/cosmos-db.bicep' = {
  name: cosmosAccountName
  params: {
    accountName: cosmosAccountName
    containerName: cosmosContainerName
    databaseName: cosmosDatabaseName
    enableFreeTier: true
    location: location
    throughput: cosmosThroughput
  }
}

module appService './bicep-modules/app-service.bicep' = {
  name: appServiceName
  params: {
    webAppName: appServiceName
    hostingPlanName: appServiceHostingPlanName
    location: location
    sku: appServiceSku
    connectionStrings: [
      {
        name: 'DefaultConnection'
        type: 'Custom'
        connectionString: cosmosDb.outputs.connectionString
      }
    ]
  }
}
