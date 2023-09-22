// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Datasync.Test.Authentication;

[ExcludeFromCodeCoverage]
public class AzureAppServiceAuthenticationHandler_Tests
{
    private const string AadValidToken = "ewogICJhdXRoX3R5cCI6ICJhYWQiLAogICJjbGFpbXMiOiBbCiAgICB7CiAgICAgICJ0eXAiOiAiYXVkIiwKICAgICAgInZhbCI6ICJlOWVkNWU1My1iYjI3LTQyMTMtODZlNC04ZDMzNDdiMTZhMzMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImlzcyIsCiAgICAgICJ2YWwiOiAiaHR0cHM6Ly9sb2dpbi5taWNyb3NvZnRvbmxpbmUuY29tL2FiY2RlZmFiLWM3YjQtNDc3My1hODk5LWJhYjJiOTdmNjg2OC92Mi4wIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJpYXQiLAogICAgICAidmFsIjogIjE2MTk3MTIyNDMiCiAgICB9LAogICAgewogICAgICAidHlwIjogIm5iZiIsCiAgICAgICJ2YWwiOiAiMTYxOTcxMjI0MyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiZXhwIiwKICAgICAgInZhbCI6ICIxNjE5NzE2MTQzIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiLAogICAgICAidmFsIjogInRlc3R1c2VyQG91dGxvb2suY29tIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJuYW1lIiwKICAgICAgInZhbCI6ICJUZXN0IFVzZXIiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL29iamVjdGlkZW50aWZpZXIiLAogICAgICAidmFsIjogImZkMTQwMGUxLTRhYjktNDM5Mi1iYjVmLTBhOThlN2MwYmQ3YyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicHJlZmVycmVkX3VzZXJuYW1lIiwKICAgICAgInZhbCI6ICJ0ZXN0dXNlckBvdXRsb29rLmNvbSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicmgiLAogICAgICAidmFsIjogIjAuQVJvQUFZQlZvclRIYzBlb21icXl1WDlvYUZOZTdla251eE5DaHVTTk0wZXhhak1TQU1FLiIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiLAogICAgICAidmFsIjogInZJTFpkN09mYmtjdkl1ZkpEVDRLQVpaczNnWmVyUnRKaWx6WG9CRDR1ZWMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudGlkIiwKICAgICAgInZhbCI6ICJhYmNkZWZhYi1jN2I0LTQ3NzMtYTg5OS1iYWIyYjk3ZjY4NjgiCiAgICB9LAogICAgewogICAgICAidHlwIjogInV0aSIsCiAgICAgICJ2YWwiOiAicmJjMGo5Mzgxa3lleExUcEhFQlFBUSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAidmVyIiwKICAgICAgInZhbCI6ICIyLjAiCiAgICB9CiAgXSwKICAibmFtZV90eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIiwKICAicm9sZV90eXAiOiAiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIgp9";
    private const string AadTokenWithNoName = "ewogICJhdXRoX3R5cCI6ICJhYWQiLAogICJjbGFpbXMiOiBbCiAgICB7CiAgICAgICJ0eXAiOiAiYXVkIiwKICAgICAgInZhbCI6ICJlOWVkNWU1My1iYjI3LTQyMTMtODZlNC04ZDMzNDdiMTZhMzMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImlzcyIsCiAgICAgICJ2YWwiOiAiaHR0cHM6Ly9sb2dpbi5taWNyb3NvZnRvbmxpbmUuY29tL2FiY2RlZmFiLWM3YjQtNDc3My1hODk5LWJhYjJiOTdmNjg2OC92Mi4wIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJpYXQiLAogICAgICAidmFsIjogIjE2MTk3MTIyNDMiCiAgICB9LAogICAgewogICAgICAidHlwIjogIm5iZiIsCiAgICAgICJ2YWwiOiAiMTYxOTcxMjI0MyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiZXhwIiwKICAgICAgInZhbCI6ICIxNjE5NzE2MTQzIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJuYW1lIiwKICAgICAgInZhbCI6ICJUZXN0IFVzZXIiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL29iamVjdGlkZW50aWZpZXIiLAogICAgICAidmFsIjogImZkMTQwMGUxLTRhYjktNDM5Mi1iYjVmLTBhOThlN2MwYmQ3YyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicHJlZmVycmVkX3VzZXJuYW1lIiwKICAgICAgInZhbCI6ICJ0ZXN0dXNlckBvdXRsb29rLmNvbSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicmgiLAogICAgICAidmFsIjogIjAuQVJvQUFZQlZvclRIYzBlb21icXl1WDlvYUZOZTdla251eE5DaHVTTk0wZXhhak1TQU1FLiIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiLAogICAgICAidmFsIjogInZJTFpkN09mYmtjdkl1ZkpEVDRLQVpaczNnWmVyUnRKaWx6WG9CRDR1ZWMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudGlkIiwKICAgICAgInZhbCI6ICJhYmNkZWZhYi1jN2I0LTQ3NzMtYTg5OS1iYWIyYjk3ZjY4NjgiCiAgICB9LAogICAgewogICAgICAidHlwIjogInV0aSIsCiAgICAgICJ2YWwiOiAicmJjMGo5Mzgxa3lleExUcEhFQlFBUSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAidmVyIiwKICAgICAgInZhbCI6ICIyLjAiCiAgICB9CiAgXSwKICAibmFtZV90eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIiwKICAicm9sZV90eXAiOiAiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIgp9";

    [Fact]
    public async Task Ctor_ForceEnable()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var handler = await GetInitializedHandler(context, options);

        // Act
        bool isEnabled = handler.IsEnabled;

        // Assert
        Assert.True(isEnabled);
    }

    [Fact]
    public async Task Ctor_EnvEnable()
    {
        // Arrange
        Environment.SetEnvironmentVariable("WEBSITE_AUTH_ENABLED", "True");
        var context = new DefaultHttpContext();
        var options = new AzureAppServiceAuthenticationOptions();
        var handler = await GetInitializedHandler(context, options);

        // Act
        bool isEnabled = handler.IsEnabled;

        // Assert
        Assert.True(isEnabled);
    }

    [Fact]
    public async Task Ctor_EnvDisable()
    {
        // Arrange
        Environment.SetEnvironmentVariable("WEBSITE_AUTH_ENABLED", null);
        var context = new DefaultHttpContext();
        var options = new AzureAppServiceAuthenticationOptions();
        var handler = await GetInitializedHandler(context, options);

        // Act
        bool isEnabled = handler.IsEnabled;

        // Assert
        Assert.False(isEnabled);
    }

    [Fact]
    public async Task HandleAuthenticate_IfDisabled_NoResult()
    {
        // Arrange
        Environment.SetEnvironmentVariable("WEBSITE_AUTH_ENABLED", null);
        var context = new DefaultHttpContext();
        var options = new AzureAppServiceAuthenticationOptions();
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticate_IfNoToken_NoResult()
    {
        // Arrange
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var context = new DefaultHttpContext();
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticate_IfBadToken_Fail()
    {
        // Arrange
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var context = new DefaultHttpContext();
        SetAuthToken(context, "aad", "some-string");
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.NotNull(result.Failure);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticate_IfNoIDP_Fail()
    {
        // Arrange
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var context = new DefaultHttpContext();
        SetAuthToken(context, null, AadValidToken);
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.NotNull(result.Failure);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticate_IfIDPDoesntMatch_Fail()
    {
        // Arrange
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var context = new DefaultHttpContext();
        SetAuthToken(context, "facebook", AadValidToken);
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.NotNull(result.Failure);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticate_IfNoName_Fail()
    {
        // Arrange
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var context = new DefaultHttpContext();
        SetAuthToken(context, "aad", AadTokenWithNoName);
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.NotNull(result.Failure);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticate_GeneratesPrincipal()
    {
        // Arrange
        var options = new AzureAppServiceAuthenticationOptions() { ForceEnable = true };
        var context = new DefaultHttpContext();
        SetAuthToken(context, "aad", AadValidToken);
        var handler = await GetInitializedHandler(context, options);

        // Act
        var result = await handler.AuthenticateAsync().ConfigureAwait(false);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(context.User);
        Assert.True(context.User.Identity.IsAuthenticated);
        Assert.Equal("testuser@outlook.com", context.User.Identity.Name);
        Assert.Equal("aad", context.User.Identity.AuthenticationType);
    }

    #region Helpers
    private static ILoggerFactory GetMockLoggerFactory()
    {
        var logger = Substitute.For<ILogger<AzureAppServiceAuthenticationHandler>>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(typeof(AzureAppServiceAuthenticationHandler).FullName).Returns(logger);
        return loggerFactory;
    }

    private static IOptionsMonitor<AzureAppServiceAuthenticationOptions> GetMockOptionsMonitor(AzureAppServiceAuthenticationOptions options)
    {
        var monitor = Substitute.For<IOptionsMonitor<AzureAppServiceAuthenticationOptions>>();
        monitor.Get(AzureAppServiceAuthentication.AuthenticationScheme).Returns(options);
        return monitor;
    }

    private static async Task<AzureAppServiceAuthenticationHandler> GetInitializedHandler(HttpContext context, AzureAppServiceAuthenticationOptions options = null)
    {
        var loggerFactory = GetMockLoggerFactory();
        var encoder = Substitute.For<UrlEncoder>();
        var clock = Substitute.For<ISystemClock>();
        var optionsMonitor = GetMockOptionsMonitor(options ?? new AzureAppServiceAuthenticationOptions());
        var authScheme = new AuthenticationScheme(AzureAppServiceAuthentication.AuthenticationScheme, AzureAppServiceAuthentication.DisplayName, typeof(AzureAppServiceAuthenticationHandler));
        var handler = new AzureAppServiceAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock);
        await handler.InitializeAsync(authScheme, context).ConfigureAwait(false);
        return handler;
    }

    private static void SetAuthToken(HttpContext context, string idp, string token)
    {
        if (idp != null)
        {
            context.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL-IDP", idp);
        }

        if (token != null)
        {
            context.Request.Headers.Add("X-MS-CLIENT-PRINCIPAL", token);
        }
    }
    #endregion
}
