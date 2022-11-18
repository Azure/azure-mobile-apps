@description('Location for all resources')
param location string = resourceGroup().location

@description('The name of the Web App')
param webAppName string

@description('The name of the App Service hosting plan')
param hostingPlanName string = 'hosting${uniqueString(resourceGroup().id)}'

@description('The SKU or Tier to use for the hosting plan')
@allowed([
  'F1', 'D1'
  'B1', 'B2', 'B3'
  'S1', 'S2', 'S3'
  'P1v2', 'P2v2', 'P3v2'
  'P1v3', 'P2v3', 'P3v3'
])
param sku string = 'F1'

@description('The number of hosting plan instances to allow')
@minValue(1)
@maxValue(16)
param capacity int = 1

@description('The list of connection strings to provide')
param connectionStrings array = []

var svcPlanTiers = {
  F1: 'Free'
  D1: 'Shared'
  B1: 'Basic'
  B2: 'Basic'
  B3: 'Basic'
  S1: 'Standard'
  S2: 'Standard'
  S3: 'Standard'
  P1v2: 'PremiumV2'
  P2v2: 'PremiumV2'
  P3v2: 'PremiumV2'
  P1v3: 'PremiumV3'
  P2v3: 'PremiumV3'
  P3v3: 'PremiumV3'
}

var svcPlanTier = svcPlanTiers[sku]
var realCapacity = sku == 'F1' || sku == 'D1' ? 1 : capacity

resource appSvcPlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: hostingPlanName
  location: location
  sku: {
    name: sku
    tier: svcPlanTier
    capacity: realCapacity
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appSvcPlan.id
    httpsOnly: true
    clientAffinityEnabled: true
    siteConfig: {
      ftpsState: 'FtpsOnly'
      phpVersion: 'off'
      minTlsVersion: '1.2'
      connectionStrings: connectionStrings
    }
  }
}

