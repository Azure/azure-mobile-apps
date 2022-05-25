Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data.Entity
Imports System.Web.Http
Imports Microsoft.Azure.Mobile.Server
Imports Microsoft.Azure.Mobile.Server.Authentication
Imports Microsoft.Azure.Mobile.Server.Config
Imports Owin

Partial Public Class Startup
    Public Sub ConfigureMobileApp(ByVal app As IAppBuilder)

        Dim config As New HttpConfiguration()

        Dim mobileConfig As New MobileAppConfiguration()
        mobileConfig _
            .UseDefaultConfiguration() _
            .ApplyTo(config)

        Database.SetInitializer(New $safeinitializerclassname$())

        Dim settings As MobileAppSettingsDictionary = config.GetMobileAppSettingsProvider().GetMobileAppSettings()

        If (String.IsNullOrEmpty(settings.HostName)) Then
            ' This middleware is intended to be used locally for debugging. By default, HostName will
            ' only have a value when running in an App Service application.
            app.UseAppServiceAuthentication(New AppServiceAuthenticationOptions() With {
                .SigningKey = ConfigurationManager.AppSettings("SigningKey"),
                .ValidAudiences = {ConfigurationManager.AppSettings("ValidAudience")},
                .ValidIssuers = {ConfigurationManager.AppSettings("ValidIssuer")},
                .TokenHandler = config.GetAppServiceTokenHandler()
            })
        End If

        app.UseWebApi(config)

    End Sub
End Class

Public Class $safeinitializerclassname$
    Inherits CreateDatabaseIfNotExists(Of $safecontextclassname$)

    Protected Overrides Sub Seed(ByVal context As $safecontextclassname$)
        Dim todoItems As List(Of TodoItem) = New List(Of TodoItem) From
            {
                New TodoItem With {.Id = Guid.NewGuid().ToString(), .Text = "First item", .Complete = False},
                New TodoItem With {.Id = Guid.NewGuid().ToString(), .Text = "Second item", .Complete = False}
            }

        For Each todoItem As TodoItem In todoItems
            context.Set(Of TodoItem).Add(todoItem)
        Next

        MyBase.Seed(context)

    End Sub
End Class
