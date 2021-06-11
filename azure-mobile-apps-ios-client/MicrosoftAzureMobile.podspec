Pod::Spec.new do |s|
  s.name         = "MicrosoftAzureMobile"
  s.version      = "3.4.0"
  s.summary      = "Client SDK for working with Azure Mobile Apps."
  s.homepage     = "http://azure.github.io/azure-mobile-apps-ios-client"
  s.license      = "Apache License, Version 2.0"
  s.author             = "Microsoft"
  s.social_media_url   = "http://twitter.com/AzureMobile"
  s.ios.deployment_target = "8.0"
  s.frameworks = "WebKit"
  s.source       = {
    :git => "https://github.com/Azure/azure-mobile-apps-ios-client.git",
    :tag => "3.4.0"
  }
  s.source_files  = "sdk/src"
  s.exclude_files = "Classes/Exclude"
  s.public_header_files = [
    "sdk/src/MicrosoftAzureMobile.h",
    "sdk/src/MSBlockDefinitions.h",
    "sdk/src/MSClient.h",
    "sdk/src/MSCoreDataStore.h",
    "sdk/src/MSDateOffset.h",
    "sdk/src/MSError.h",
    "sdk/src/MSFilter.h",
    "sdk/src/MSManagedObjectObserver.h",
    "sdk/src/MSPullSettings.h",
    "sdk/src/MSConnectionConfiguration.h",
    "sdk/src/MSPush.h",
    "sdk/src/MSInstallation.h",
    "sdk/src/MSInstallationTemplate.h",
    "sdk/src/MSQuery.h",
    "sdk/src/MSQueryResult.h",
    "sdk/src/MSSyncContext.h",
    "sdk/src/MSSyncContextReadResult.h",
    "sdk/src/MSSyncTable.h",
    "sdk/src/MSTable.h",
    "sdk/src/MSTableOperation.h",
    "sdk/src/MSTableOperationError.h",
    "sdk/src/MSUser.h"
  ]
  s.ios.public_header_files = [
    "sdk/src/MSLoginController.h",
  ]
  s.requires_arc = true
end
