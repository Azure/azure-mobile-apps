# Azure Mobile Apps JavaScript SDK Change Log

### 2.0.0 (azure-mobile-apps-client npm package)
- ** __BREAKING CHANGE__ ** Entry module does not export `MobileServiceTable`, `MobileServiceSyncTable` and `MobileServiceLogin` anymore [41b31e3](https://github.com/Azure/azure-mobile-apps-js-client/commit/41b31e31f73b01b8cf358afa7f46cbf31d1cc074)
- Miscellaneous bug fixes

### 2.0.0 (cordova-plugin-ms-azure-mobile-apps Cordova plugin)
- ** __BREAKING CHANGE__ ** Entry module does not export `MobileServiceTable`, `MobileServiceSyncTable` and `MobileServiceLogin` anymore [41b31e3](https://github.com/Azure/azure-mobile-apps-js-client/commit/41b31e31f73b01b8cf358afa7f46cbf31d1cc074)
- Miscellaneous bug fixes

### 2.0.0-rc1  (azure-mobile-apps-client npm package)
- [Issue #88](https://github.com/Azure/azure-mobile-apps-js-client/issues/88) and [Issue #158](https://github.com/Azure/azure-mobile-apps-js-client/issues/158) - `npm install azure-mobile-apps-client` installs the package in original source form, i.e. without bundling it [08de350](https://github.com/Azure/azure-mobile-apps-js-client/commit/08de350b858b6adafe25cd985a924769bd6cef42)
- Miscellaneous bug fixes

### cordova-2.0.0-rc1 (cordova-plugin-ms-azure-mobile-apps Cordova plugin)
- ** __BREAKING CHANGE__ ** - `onConflict(serverRecord, clientRecord, pushError)` method of `pushError` has been changed to `onConflict(pushError)` [ f1167bc ](https://github.com/Azure/azure-mobile-apps-js-client/commit/f1167bc0f656241e13e867b3ebc7170d93492df5)
- [Issue #145](https://github.com/Azure/azure-mobile-apps-js-client/issues/145) - Bug fix: Invalid push error handling can cause push to run endlessly [ a2287d4 ](https://github.com/Azure/azure-mobile-apps-js-client/commit/a2287d41cda16cdbd35ccc8bba83041e8cdb30a8)
- [Issue #179](https://github.com/Azure/azure-mobile-apps-js-client/issues/179) - Fix conflict detection while pushing delete operations [ 366f055](https://github.com/Azure/azure-mobile-apps-js-client/commit/366f0551aabc1b220502727ccf46d81f1ef71469)

### cordova-2.0.0-beta6 (cordova-plugin-ms-azure-mobile-apps Cordova plugin)
- [Issue #135](https://github.com/Azure/azure-mobile-apps-js-client/issues/135) - Pull supports custom page size [ 34b9be5](https://github.com/Azure/azure-mobile-apps-js-client/commit/34b9be55a4432af78501b3028b728790aa89ca0b)
- [Issue #131](https://github.com/Azure/azure-mobile-apps-js-client/issues/131) - Updated store to be insensitive to table and column name casing [4958927](https://github.com/Azure/azure-mobile-apps-js-client/commit/49589276c6ebdb792455d0e5dd087ac908d30c50)
- [Issue #128](https://github.com/Azure/azure-mobile-apps-js-client/issues/128) - Added API to close underlying database connection [8347c08](https://github.com/Azure/azure-mobile-apps-js-client/commit/8347c08e3f02ed1aff57c03ac8ea0de4a7065cc7)
- [Issue #38](https://github.com/Azure/azure-mobile-apps-js-client/issues/38) - Added API to purge store operations [66630d5](https://github.com/Azure/azure-mobile-apps-js-client/commit/66630d50ca915f9b0387def10fc0fe3017c896a1)

### cordova-2.0.0-beta5 (cordova-plugin-ms-azure-mobile-apps Cordova plugin)
- Added support for offline data sync (preview)

### 2.0.0-beta5 (azure-mobile-apps-client npm package)
- Updated the SDK to be a UMD compliant npm package

### 2.0.0-beta4 (azure-mobile-apps-client npm package)
- Fixed authentication for Apache Cordova apps based on Ionic Framework or running in browser environments
- Updated the SDK to be an npm package

### 1.2.7
- Added support for phonegap/cordova with [plugin repo](https://github.com/Azure/azure-mobile-services-cordova)

### 1.2.5
- Added support for sending provider specific query string parameters in login using new loginWithOptions method
- Added support for registering devices with notification hubs for apns and gcm
- Fixed issue with InAppBrowser on iOS devices during auth workflows when using Cordova/PhoneGap

### 1.2.4
- Fixed crash when server response did not have a Content-Type header

### 1.2.2 
- Support for optimistic concurrency on delete

### 1.1.5
- Fix issue [#218](https://github.com/WindowsAzure/azure-mobile-services/issues/218) in which some dates coming from the mobile services with the .NET runtime weren't parsed correctly
- [WinJS only] Fix race condition on notification hub integration initialization when storage was corrupted

### 1.1.4
- Added support for Windows Azure Notification Hub integration for WinJS.

### 1.1.3
- Added a mapping in the authentication provider from WindowsAzureActiveDirectory to the value used in the REST API (`/login/aad`)

### 1.1.2
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns

### 1.1.0
- Support for tables with string ids
- Removed client restriction on valid providers for login
- Files are now served from http://ajax.aspnetcdn.com/ajax/mobileservices/MobileServices.Web-[version].min.js (or [version].js for the non minified copy)

### 1.0.3:
- Added support for `String.substr` inside functions on `where` clauses
- Fix [#152](https://github.com/WindowsAzure/azure-mobile-services/issues/152) - InvokeApi method crashes on IE9 and IE8
- Fixed issue with login popup not being closed when using IE11
