targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

//@description('Id of the user or app to assign application roles')
//param principalId string = ''

var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}-${resourceToken}'
  location: location
  tags: tags
}

module service './service.bicep' = {
  name: 'service'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
//    principalId: principalId
    resourceToken: resourceToken
    tags: tags
  }
}
