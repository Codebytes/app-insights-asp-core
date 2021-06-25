param appName string
param location string = resourceGroup().location

var appInsightName = toLower('appi-${appName}')
var appUniqueName = toLower('${appName}-${uniqueString(resourceGroup().id)}')
resource appInsights 'microsoft.insights/components@2020-02-02-preview' = {
  name: appInsightName
  location: location
  kind: 'string'
  properties: {
      Application_Type: 'web'
  }
}
 
resource hosting 'Microsoft.Web/serverfarms@2019-08-01' = {
    name: 'hosting-${appName}'
    location: location
    sku: {
        name: 'S1'
    }
}
 
resource app 'Microsoft.Web/sites@2018-11-01' = {
    name: appUniqueName
    location: location
    identity: {
        type: 'SystemAssigned'
    }
    properties: {
        siteConfig: {
            appSettings: [
                {
                    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
                    value: appInsights.properties.InstrumentationKey
                }
                {
                    name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
                    value: '~2'

                }
                {
                    name: 'XDT_MicrosoftApplicationInsights_Mode'
                    value: 'recommended'
                }
                {
                    name: 'InstrumentationEngine_EXTENSION_VERSION'
                    value: '~1'
                }

                {
                    name: 'XDT_MicrosoftApplicationInsights_BaseExtensions'
                    value: '~1'
                }
                {
                    name: 'XDT_MicrosoftApplicationInsights_PreemptSdk'
                    value: '1'
                }
                {
                    name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
                    value: '1.0.0'
                }
                {
                    name: 'APPINSIGHTS_SNAPSHOTFEATURE_VERSION'
                    value: '1.0.0'
                }
                {
                    name: 'DiagnosticServices_EXTENSION_VERSION'
                    value:'~3'
                }
            ]
        }
        serverFarmId: hosting.id
    }
}

output appName string = appUniqueName
output appId string = app.id
output APPINSIGHTS_INSTRUMENTATIONKEY string = appInsights.properties.InstrumentationKey
