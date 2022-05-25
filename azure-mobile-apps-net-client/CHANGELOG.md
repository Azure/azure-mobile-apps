# Azure Mobile Apps Managed SDK Change Log

## Version 4.2.0

- Added support for .NET Standard 2.0, Xamarin.Android 9.0, Xamarin.iOS 1.0
- Updated support for UAP to builds 16299 through 19041.
- **Breaking** Removed support for .NET Standard 1.4 and PCL editions
- **Breaking** Update of Newtonsoft.JSON to v12.0.3 - this causes some changes in how the JSON parser handles overflow conditions.
- **Breaking** You can no longer create a `MobileServiceUser` with an invalid or illegal username (such as null or the empty string).
- **Breaking** Switched to using Xamarin.Essentials for authentication broker.  This requires changes to the per-platform setup.
- Update of SQLitePCL to v2.0.4
- Moved all non-live tests to use MSTest v2
- Push technologies / registration is no longer tested
- [#511](https://github.com/Azure/azure-mobile-apps-net-client/issues/511) Support for Visual Studio 2019
- [#415](https://github.com/Azure/azure-mobile-apps-net-client/issues/415) Support for latest Xamarin iOS and Android releases.
- [#543](https://github.com/Azure/azure-mobile-apps-net-client/pull/543) Check compression header of response
- [#300](https://github.com/Azure/azure-mobile-apps-net-client/issues/300) Options based client configuration for improved dependency injection configuration.
- [#539](https://github.com/Azure/azure-mobile-apps-net-client/issues/539) Removed dependency on Xamarin.Auth, which removes dependency on UIWebView.
- [#533](https://github.com/Azure/azure-mobile-apps-net-client/issues/533) All packages used have been updated to latest editions, which should alleviate compatibility issues.

## Version 4.1.2

- [#488](https://github.com/Azure/azure-mobile-apps-net-client/pull/488) Fix canceling callback in iOS login view.
- [#524](https://github.com/Azure/azure-mobile-apps-net-client/pull/524) Fix parsing token for Push notifications on iOS 13.
- [#526](https://github.com/Azure/azure-mobile-apps-net-client/pull/526) Fix the validation of HTTP responses with AutomaticDecompression enabled.

## Version 4.1.1

- Fix assembly signing.

## Version 4.1.0

- [#409](https://github.com/Azure/azure-mobile-apps-net-client/pull/409) Fix a bug that causes `InvalidCastException` in `PullAsync`.
- [#439](https://github.com/Azure/azure-mobile-apps-net-client/pull/439) Fix a parameter validation bug in `MobileServiceSyncTable`.
- [#443](https://github.com/Azure/azure-mobile-apps-net-client/pull/443) [#449](https://github.com/Azure/azure-mobile-apps-net-client/pull/449) Fix a bug that missing operation item is not added to sync errors and causing `NullReferenceException` in offline sync.
- [#448](https://github.com/Azure/azure-mobile-apps-net-client/pull/448) Fix a i18n bug of parsing number literal where different locale could have different decimal separator.
- [#458](https://github.com/Azure/azure-mobile-apps-net-client/pull/458) Treat `TimeoutException` as network error in `MobileServiceHttpClient`.
- [#478](https://github.com/Azure/azure-mobile-apps-net-client/pull/478) Update 'Microsoft.NETCore.UniversalWindowsPlatform' package to v5.2.6. 

## Version 4.0.2

- Support [#338](https://github.com/Azure/azure-mobile-apps-net-client/pull/338) PCL-based Xamarin.Forms projects. PCL-based projects were previously supported in v3.1.0 but not in v4.0.1. They are now supported in v4.0.2.
- Fix [#339](https://github.com/Azure/azure-mobile-apps-net-client/pull/339) a bug of RedirectUrlActivity in authentication
- Fix [Azure/azure-mobile-services-xamarin-auth/11](https://github.com/Azure/azure-mobile-services-xamarin-auth/pull/11) LaunchUrl method missing

## Version 4.0.1

- Fix [#339](https://github.com/Azure/azure-mobile-apps-net-client/pull/339) Check null in RedirectUrlActivity
- Fix [#348](https://github.com/Azure/azure-mobile-apps-net-client/issues/348) Downgrade Xamarin.Android.Support.CustomTabs to 23.3.0

## Version 4.0.0

- Support of .NET Standard 1.4
- [Xamarin.Android] Support of server login flow to use Chrome CustomTabs on Android. It supports OAuth 2.0 [PKCE](https://tools.ietf.org/html/rfc7636) extension.
- [Xamarin.iOS] Support of server login flow to use SafariViewController on iOS. It supports OAuth 2.0 [PKCE](https://tools.ietf.org/html/rfc7636) extension.
- [UWP] Support of server login flow to use browser on Windows. It supports OAuth 2.0 [PKCE](https://tools.ietf.org/html/rfc7636) extension.

## Version 3.1.0

- Fix issue [#193](https://github.com/Azure/azure-mobile-apps-net-client/issues/193)
- Fix issue [#196](https://github.com/Azure/azure-mobile-apps-net-client/issues/196)
- Fix issue [#215](https://github.com/Azure/azure-mobile-apps-net-client/issues/215)
- Fix issue [#237](https://github.com/Azure/azure-mobile-apps-net-client/issues/237)

## Version 3.0.3

- Fix [#244](https://github.com/Azure/azure-mobile-apps-net-client/issues/244) Exception if not specifying full MobileServiceSQLiteStore path

## Version 3.0.2

- Fix [#233](https://github.com/Azure/azure-mobile-apps-net-client/issues/233) Remove requirement to specify android path/create file
- Fix [#236](https://github.com/Azure/azure-mobile-apps-net-client/issues/236) Call SQLitePCL.Batteries.Init() only once
- Fix [#240](https://github.com/Azure/azure-mobile-apps-net-client/issues/240) Relative paths don't work on Android and iOS for SQLiteStore
- Update documentation and README

## Version 3.0.1

- Fix [#228](https://github.com/Azure/azure-mobile-apps-net-client/issues/228) Assembly version of Microsoft.WindowsAzure.Mobile.SQLiteStore.dll in [Microsoft.Azure.Mobile.Client.SQLiteStore 3.0.0 package](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.SQLiteStore/3.0.0) is wrong

## Version 3.0.0

- Update [Microsoft.Azure.Mobile.Client.SQLiteStore](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.SQLiteStore) to depend on [SQLitePCLRaw.core](https://www.nuget.org/packages/SQLitePCLRaw.core/) and [SQLitePCLRaw.bundle_green](https://www.nuget.org/packages/SQLitePCLRaw.bundle_green) instead of [SQLitePCL](https://www.nuget.org/packages/SQLitePCL)
- Fix [#210](https://github.com/Azure/azure-mobile-apps-net-client/issues/210) NuGet package should declare System.Net.Http as a framework assembly
- Fix [#221](https://github.com/Azure/azure-mobile-apps-net-client/issues/221) Specify monotouch and monoandroid framework version in Microsoft.Azure.Mobile.Client.SQLiteStore package

## Version 2.1.1

- Fixing [#186](https://github.com/Azure/azure-mobile-apps-net-client/issues/186) threading issues with MobileServiceContractResolver. [2feba7c](https://github.com/Azure/azure-mobile-apps-net-client/commit/2feba7c3cf6430f1f55878c7945251a17c1376dd)
- Fixing [#178](https://github.com/Azure/azure-mobile-apps-net-client/issues/178) missing IntelliSense of [Microsoft.WindowsAzure.Mobile.SQLiteStore nuget](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.sqlitestore). [88a1ef4](https://github.com/Azure/azure-mobile-apps-net-client/commit/88a1ef4e0783ca0878486e0ff81050153e11baed)

## Version 2.1.0

- Support [#110](https://github.com/Azure/azure-mobile-apps-net-client/issues/110) Auth Token Refresh [eddaee7](https://github.com/Azure/azure-mobile-apps-net-client/commit/eddaee72823b9f2e145b7b070f7e12bb45aa01c5)
- Fix Single Sign-on LoginAsync malformatted query url issue in Windows Store 8 and Windows Phone 8.1 [b6c24c7](https://github.com/Azure/azure-mobile-apps-net-client/commit/b6c24c7389af5c20394091b8cdd077c2a79d025e)
- Fix [#25](https://github.com/Azure/azure-mobile-apps-net-client/issues/25) Convert JObject to string for xamarin ios and andriod template parameter's body [6a1c60d](https://github.com/Azure/azure-mobile-apps-net-client/commit/6a1c60dc73bdcd9db2942fdedd381ca126071c7b)

## Version 2.0.1

- Updated Error.UpdateAsync to use the tracked store [3673d02](https://github.com/Azure/azure-mobile-apps-net-client/commit/3673d02)
- Fixing issues when using Xamarin linker [dc5a101](https://github.com/Azure/azure-mobile-apps-net-client/commit/dc5a101)
- Serializing access to the transaction scope to prevent issues with SQLite [a91a095](https://github.com/Azure/azure-mobile-apps-net-client/commit/a91a095)
- Nuget package updates [6bc545b](https://github.com/Azure/azure-mobile-apps-net-client/commit/6bc545b)

## Version 2.0.0

- Azure Mobile Apps release

## Version 2.0.0-beta

- Support for Azure Mobile Apps

## Version 1.3.2

- Added workaround for WinRT issue [#658](https://github.com/WindowsAzure/azure-mobile-services/issues/658) by removing localization in SQLiteStore and in the SDK  [6af8b30](https://github.com/Azure/azure-mobile-services/commit/6af8b30) [58c5a44](https://github.com/Azure/azure-mobile-services/commit/58c5a44)
- Added partial fix for issue [#615](https://github.com/WindowsAzure/azure-mobile-services/issues/615), by removing operations from the queue before releasing the operation's lock. [a28ae32](https://github.com/Azure/azure-mobile-services/commit/a28ae32)

## Version 1.3.1

- Update to latest version of sqlite pcl [ce1aa67](https://github.com/Azure/azure-mobile-services/commit/ce1aa67)
- Fix iOS classic compilation issues [316a57a](https://github.com/Azure/azure-mobile-services/commit/316a57a)
- Update Xamarin unified support for Xamarin.iOS 8.6
[da537b1](https://github.com/Azure/azure-mobile-services/commit/da537b1)
- Xamarin.iOS Unified API Support [d778c60](https://github.com/Azure/azure-mobile-services/commit/d778c60)
- Relax queryId restrictions #521 [offline]
[3e2f645](https://github.com/Azure/azure-mobile-services/commit/3e2f645)
- Work around for resource missing error on windows phone [offline]

## Version 1.3

- allow underscore and hyphen in queryId [7d192a3](https://github.com/Azure/azure-mobile-services/commit/7d192a3)
- added force option to purge data and pending operations on data [aa51d9f](https://github.com/Azure/azure-mobile-services/commit/aa51d9f)
- delete errors with operation on cancel and collapse [372ba61](https://github.com/Azure/azure-mobile-services/commit/372ba61)
- rename queryKey to queryId [93e59f7](https://github.com/Azure/azure-mobile-services/commit/93e59f7)
- insert should throw if the item already exists [#491](https://github.com/Azure/azure-mobile-services/issues/491) [fc13891](https://github.com/Azure/azure-mobile-services/commit/fc13891)
- **Breaking** Removed PullAsync overloads that do not take queryId [88cac8c](https://github.com/Azure/azure-mobile-services/commit/88cac8c)

## Version 1.3 beta3

- Improved the push failure error message [d49a72e](https://github.com/Azure/azure-mobile-services/commit/d49a72e)
- Implement true upsert [c5b0b38](https://github.com/Azure/azure-mobile-services/commit/c5b0b38)
- Use more fine grained types in sqlite store [de49712](https://github.com/Azure/azure-mobile-services/commit/de49712)
- Speedup store table creation [eb7cc8d](https://github.com/Azure/azure-mobile-services/commit/eb7cc8d)
- Allow query on member name datetime [7d831cd](https://github.com/Azure/azure-mobile-services/commit/7d831cd)
- Make the sync handler optional as there is alternate way for handling sync errors [edc04e5](https://github.com/Azure/azure-mobile-services/commit/edc04e5)
- Drop the unused createdat column in operations table [8a30df4](https://github.com/Azure/azure-mobile-services/commit/8a30df4)
- Remove redundant overloads in interface and move them to extensions [d0a46b6](https://github.com/Azure/azure-mobile-services/commit/d0a46b6)
- Support relative and absolute uri in pull same as table.read [c9d8e39](https://github.com/Azure/azure-mobile-services/commit/c9d8e39)
- Allow relative URI in invokeapi [5b3c6b3](https://github.com/Azure/azure-mobile-services/commit/5b3c6b3)
- Fixed the like implementation in sqlite store [77a0180](https://github.com/Azure/azure-mobile-services/commit/77a0180)
- Purge should forget the deltatoken [18f1803](https://github.com/Azure/azure-mobile-services/commit/18f1803)
- Renamed fromServer to ignoreMissingColumns [8b047eb](https://github.com/Azure/azure-mobile-services/commit/8b047eb)
- **Breaking** Removed PullAsync overloads that do not take queryKey [d4ff784](https://github.com/Azure/azure-mobile-services/commit/d4ff784)
- Save tableKind in the errors table [23f2ef0](https://github.com/Azure/azure-mobile-services/commit/23f2ef0)

## Version 1.3 beta2

- Updated Nuget references
- Request __deleted system property for sync
- Default delta token set to 1970-01-01 for compatibility with Table Storage
- Expose protected methods from the MobileServiceSQLiteStore for intercepting sql
- **Breaking** Expose a ReadOnlyCollection instead of IEnumerable from MobileServiceTableOperationError

## Version 1.3 beta

- Added support for incremental sync for .NET backend
- Added support for byte[] properties in offline
- Fixed issue with timezone roundtripping in incremental sync
- Improved exception handling for 409 conflicts
- Improved error handling for timeout errors during sync
- Follow link headers returned from .NET backend and use skip and top for PullAsync()
- Introduced the SupportedOptions enum on IMobileServiceSyncTable to configure the pull strategy
- **Breaking** Do not Push changes on PurgeAsync() instead throw an exception
- **Breaking** Renamed ToQueryString method to ToODataString on MobileServiceTableQueryDescription class

## Version 1.3 alpha2

- Added support for incremental sync (currently, for Mobile Services JavaScript backend only)
- Added client support for soft delete
- Added support for offline pull with query string

## Version 1.3 alpha1

- Added support for offline and sync
- Added support for soft delete

## Version 1.2.6

- Fixed an issue on Xamarin.iOS and Xamarin.Android where UI popups occur during failed user authentication flows. These popups are now suppressed so that the developer can handle the error however they want.

## Version 1.2.5

- Updated to use a modified build of Xamarin.Auth that will not conflict with any user-included version of Xamarin.Auth

## Version 1.2.4

- Added support for following link headers returned from the .NET backend
- Added a MobileServiceConflictException to detect duplicate inserts
- Added support for datetimeoffsets in queries
- Added support for sending provider specific query string parameters in LoginAsync()
- Fixed an issue causing duplicate registrations in Xamarin.iOS against .NET backends

## Version 1.2.3

- Added support for Xamarin iOS Azure Notification Hub integration

## Version 1.2.2

- Support for optimistic concurrency on delete
- Update to Push surface area with minor object model changes. Added Registration base class in PCL and changed name within each extension to match the push notifcation surface. Example: WnsRegistration, WnsTemplateRegistration
- Added support for Xamarin Android Azure Notification Hub integration

## Version 1.2.1

- Added support for Windows Phone 8.1, requires using Visual Studio 2013 Update 2 RC

## Version 1.1.5

- Added support for Xamarin (iOS / Android)
- Clean-up id validation on insert operations

## Version 1.1.4

- Added support for Windows Azure Notification Hub integration.

## Version 1.1.3

- Added support for the Windows Azure Active Directory authentication in the `MobileServiceAuthenticationProvider` enumeration.
- Also added a mapping from that name to the value used in the service REST API (`/login/aad`)
- Fixed a issue [#213](https://github.com/WindowsAzure/azure-mobile-services/issues/213) in which SDK prevented calls to custom APIs with query string parameters starting with `$`

## Version 1.1.2

- Fix [#192](https://github.com/WindowsAzure/azure-mobile-services/issues/192) - Serialized query is ambiguous if double literal has no fractional part
- Fixed Nuget support for Windows Phone 8

## Version 1.1.1

- Fix bug when inserting a derived type
- Dropped support for Windows Phone 7.x clients (WP7.5 can still use the client version 1.1.0)

## Version 1.1.0

- Support for tables with string ids
- Support for optimistic concurrency (version / ETag) validation
- Support for `__createdAt` / `__updatedAt` table columns
- Overload for log in which takes the provider as a string, in addition to the one with enums
- Fix [#121](https://github.com/WindowsAzure/azure-mobile-services/issues/121) - exceptions in `MobileServiceIncrementalLoadingCollection.LoadMoreItemsAsync` causes the app to crash

## Version 1.0.3

- Fixed query issues in Visual Basic expressions
