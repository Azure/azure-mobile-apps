targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

param appServiceName string = ''
param appServicePlanName string = ''
param resourceGroupName string = ''
param sqlServerName string = ''
param sqlDatabaseName string = ''

@description('SQL Server administrator password')
param sqlAdminUsername string

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : 'rg-${environmentName}'
  location: location
  tags: tags
}

module resources './resources.bicep' = {
  name: 'resources'
  scope: resourceGroup
  params: {
    location: location
    tags: tags
    appServiceName: !empty(appServiceName) ? appServiceName : 'app-${resourceToken}'
    appServicePlanName: !empty(appServicePlanName) ? appServicePlanName : 'asp-${resourceToken}'
    sqlServerName: !empty(sqlServerName) ? sqlServerName : 'sql-${resourceToken}'
    sqlDatabaseName: !empty(sqlDatabaseName) ? sqlDatabaseName : 'TodoDb'
    sqlAdminUsername: !empty(sqlAdminUsername) ? sqlAdminUsername : 'appadmin'
    sqlAdminPassword: sqlAdminPassword
  }
}

output SQL_CONNECTION_STRING string = resources.outputs.SQL_CONNECTION_STRING
output SERVICE_ENDPOINT string = resources.outputs.SERVICE_ENDPOINT
output SQL_ADMIN_USERNAME string = resources.outputs.SQL_ADMIN_USERNAME
#disable-next-line outputs-should-not-contain-secrets
output SQL_ADMIN_PASSWORD string = resources.outputs.SQL_ADMIN_PASSWORD
