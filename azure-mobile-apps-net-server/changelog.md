This is the changelog for the Azure Mobile **Apps** .NET server SDK, which are in the nuget packages **Microsoft.Azure.Mobile.Server.***

Note: this applies only to the new Server SDK that's designed for App Service, **not Mobile Services**. Mobile Apps is only available in the new Azure portal.

_____________

**Release 2.0.3 (December 2018)**

- Update SDKs to v2.0.3 to fix a strong name issue with v2.0.1 and v2.0.2 assemblies. (The issue was not properly addressed in v2.0.2.)

**Release 2.0.2 (December 2018)**

- [#262](https://github.com/Azure/azure-mobile-apps-net-server/pull/262): Update SDKs to v2.0.2 to fix a strong name issue with v2.0.1 Nugets.

**Release 2.0.1 (December 2018)**

- [#190](https://github.com/Azure/azure-mobile-apps-net-server/pull/190): Add overload for GetIdentitiesAsync that only requires the X-ZUMO-AUTH token value.

- [#205](https://github.com/Azure/azure-mobile-apps-net-server/pull/205): Check `System.IdentityModel.Tokens.Jwt` dependency version is higher than 4.0.3 and lower than 5.0.0.

- [#245](https://github.com/Azure/azure-mobile-apps-net-server/pull/245): Update out-of-date JQuery used in the landing page.

- [#259](https://github.com/Azure/azure-mobile-apps-net-server/pull/259): Update 'Microsoft.Data.OData' dependency to v5.8.4.

**Release 2.0.0 (January 2017)**
[https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/2.0.0](https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/2.0.0)

- Fixed [Issue #164](https://github.com/Azure/azure-mobile-apps-net-server/issues/164): Support .NET Framework 4.6 as a base version

- Fixed [Issue #173](https://github.com/Azure/azure-mobile-apps-net-server/issues/173): Remove the Push Registration Endpoint

- Fixed [Issue #171](https://github.com/Azure/azure-mobile-apps-net-server/issues/171): Mobile Apps Swagger NuGet package not available

**Release 1.1.157.1 (February 2016)**
[https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/1.1.157.1](https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/1.1.157.1)

- Introducing Swagger support with **Microsoft.Azure.Mobile.Server.Swagger**, currently in Preview. For more details, see [Adding Swagger Metadata and Help UI to a Mobile App](https://github.com/Azure/azure-mobile-apps-net-server/wiki/Adding-Swagger-Metadata-and-Help-UI-to-a-Mobile-App)

- Fixed [Issue #40](https://github.com/Azure/azure-mobile-apps-net-server/issues/40): TableControllers appear on both the /api and the /tables routes

- Fixed [Issue #48](https://github.com/Azure/azure-mobile-apps-net-server/issues/48): Improved configuration for MobileAppControllers. See [Configuring MobileAppController and TableController Settings](https://github.com/Azure/azure-mobile-apps-net-server/wiki/Configuring-MobileAppController-and-TableController-Settings) for more information.

- Fixed [Issue #59](https://github.com/Azure/azure-mobile-apps-net-server/issues/59): AccessToken is null

- Fixed [Issue #70](https://github.com/Azure/azure-mobile-apps-net-server/issues/70): IPrincipal.GetAppServiceIdentityAsync drops claims with same names.

**Release 1.0.119 (December 2015 | GA)**
[https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/1.0.119.0](https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/1.0.119.0)

- (Breaking change) Renamed `MobileAppTokenHandler` (and related methods) to `AppServiceTokenHandler`

- (Breaking change) Removed unused `HttpConfiguration` and `HttpRequestMessage` extension methods

- (Breaking change) Removed the public `MobileAppSettingsProvider` property on `MobileAppConfiguration`

**Release 0.3.36 (November 2015 | RC)**
[https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/0.3.36.0](https://github.com/Azure/azure-mobile-apps-net-server/releases/tag/0.3.36.0)

For details, see this blog post: [Azure Mobile Apps November 2015 Update](http://go.microsoft.com/fwlink/?LinkId=703717)

- (New!) Added Login package for creating tokens that can be authenticated by App Service Authentication.

- (Breaking change) System properties no longer start with double underscores (__)

- (Breaking change) ZUMO-API-VERSION header or query string parameter with value of 2.0.0 is now required

- (Breaking change) Removed `MobileAppUser`. Authenticated users are now of type `ClaimsPrincipal`.

- (Breaking change) `AppServiceAuthenticationMiddleware` now mimics App Service Authentication; this middleware should now only be used during local debugging.

- (Breaking change) Removed `SigningKey` from `MobileAppSettingsDictionary`.

- (Breaking change) `GetIdentityAsync<>()` has been removed in favor of `GetAppServiceIdentityAsync<>()`, an extension method on `IPrincipal`. The new method targets App Service Authentication rather than the Gateway.

- (Breaking change) Any classes starting with `MobileAppAuth` have been renamed `AppServiceAuth`.

**Release 0.2.575 (September 2015)**

[https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server/0.2.575](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server/0.2.575)

For details, see this blog post: [http://azure.microsoft.com/en-us/blog/azure-mobile-apps-september-2015-update/](http://azure.microsoft.com/en-us/blog/azure-mobile-apps-september-2015-update/)

- (New!) Support for Azure Table Storage

- (Breaking change) Removed automatic OWIN setup, to better integrate with ASP.NET. You should now add your own OWIN startup class.

- (Breaking change) Changed authentication middleware setup. The method `MobileAppConfiguration.AddAppServiceAuthentication()` has been removed. You should instead call the method `IAppBuilder.UseMobileAppAuthentication(HttpConfiguration)` in your OWIN startup class.

- (Breaking change) Removed `MobileAppSettingsDictionary.GetSchemaName()`

- (Server quickstart change) web.config no longer specifes the key and value for `MS_MobileServiceName`.

- (Server quickstart change) Removed the `Global.asax` files and `App_Start/WebApiConfig.cs`. Added the OWIN startup class `Startup.cs` and `App_Start/Startup.MobileApp.cs`.

- Note: you can view the latest server quickstart on GitHub here: [https://github.com/Azure/azure-mobile-services-quickstarts/tree/MobileApp/backend/dotnet/Quickstart](https://github.com/Azure/azure-mobile-services-quickstarts/tree/MobileApp/backend/dotnet/Quickstart)

**Release 0.2.553 (August 2015)**  

[https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server/0.2.553](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server/0.2.553)  
For details, see this blog post: [http://azure.microsoft.com/en-us/blog/azure-mobile-apps-august-2015-update/](http://azure.microsoft.com/en-us/blog/azure-mobile-apps-august-2015-update/)

- (Breaking change) ServiceSettingsDictionary has been renamed to MobileAppSettingsDictionary
  
  From a table controller method, you would use this code:  

      MobileAppSettingsDictionary settings =  
           this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();

- (Bugfix) System properties now have the correct json serializer settings applied

**Release 0.2.549 (July 2015)**  

[https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server/0.2.549](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server/0.2.549)  
For more details, see this blog post: [http://azure.microsoft.com/en-us/blog/updates-to-the-mobile-apps-server-sdk/](http://azure.microsoft.com/en-us/blog/updates-to-the-mobile-apps-server-sdk/)

- (New) Server nugets are now split up so you can use just the parts you need. Added new NuGet package [Microsoft.Azure.Mobile.Server.Quickstart](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server.Quickstart/), with these dependencies:

  * [Microsoft.Azure.Mobile.Server.Notifications](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server.Notifications/)
  * [Microsoft.Azure.Mobile.Server.CrossDomain](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server.CrossDomain/)
  * [Microsoft.Azure.Mobile.Server.Entity](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server.Entity/)
  * [Microsoft.Azure.Mobile.Server.Home](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Server.Home/)


- (Breaking change) Removed the custom Mobile `[AuthorizeLevel]` attribute. You should now decorate controllers with the standard ASP.NET `[Authorize]` attribute. 

- (Breaking change) Application Key and Master Key are no longer used by the SDK. We also no longer assume Application Key protection for APIs by default. The reason for the change is that an Application Key provides no real protection, and a Master Key should never be distributed for an app. We recommend that you use user authentication using the `[Authorize]` attribute.

- (Breaking change) By default, ApiControllers are no longer mapped to a route or given any mobile-specific configuration. To designate an ApiController as a mobile controller, specify the [MobileAppController] attribute on your class.

- (Feature change) The "Try out out" autogenerated help page is no longer available, since we removed the Master Key setting. We're bringing it back in a future release!  In the meantime, we recommend using a REST testing tool like [Fiddler](http://www.telerik.com/download/fiddler) or [Postman](https://chrome.google.com/webstore/detail/postman/fhbjgbiflinjbdggehcddcbncdddomop?hl=en).
