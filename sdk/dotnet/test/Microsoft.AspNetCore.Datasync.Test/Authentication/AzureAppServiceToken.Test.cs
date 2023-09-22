// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using System.Text;

namespace Microsoft.AspNetCore.Datasync.Test.Authentication;

[ExcludeFromCodeCoverage]
public class AzureAppServiceToken_Tests
{
    [Fact]
    public void FromString_Null_Throws()
    {
        // Arrange
        const string token = null;

        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_Empty_Throws()
    {
        // Arrange
        const string token = "";

        // Act + Assert
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_NotB64_Throws()
    {
        // Arrange
        const string token = "some-random-string";

        // Act + Assert
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_NotJson_Throws()
    {
        // Arrange
        const string text = "some-string";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act + Assert
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_PopulatesProvider()
    {
        // Arrange
        const string text = "{\"auth_typ\":\"aad\"}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        var actual = AzureAppServiceToken.FromString(token);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("aad", actual.Provider);
        Assert.Null(actual.NameType);
        Assert.Null(actual.RoleType);
        Assert.Null(actual.Claims);
    }

    [Fact]
    public void FromString_PopulatesNameType()
    {
        // Arrange
        const string text = "{\"name_typ\":\"aad\"}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        var actual = AzureAppServiceToken.FromString(token);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("aad", actual.NameType);
        Assert.Null(actual.Provider);
        Assert.Null(actual.RoleType);
        Assert.Null(actual.Claims);
    }

    [Fact]
    public void FromString_PopulatesRoleType()
    {
        // Arrange
        const string text = "{\"role_typ\":\"aad\"}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        var actual = AzureAppServiceToken.FromString(token);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("aad", actual.RoleType);
        Assert.Null(actual.Provider);
        Assert.Null(actual.NameType);
        Assert.Null(actual.Claims);
    }

    [Fact]
    public void FromString_ObjectForClaims_Throws()
    {
        // Arrange
        const string text = "{\"claims\":{\"aad\":\"bar\"}}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_StringForClaims_Throws()
    {
        // Arrange
        const string text = "{\"claims\":\"aad\"}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_NumberForClaims_Throws()
    {
        // Arrange
        const string text = "{\"claims\":42}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_BadClaim_Throws()
    {
        // Arrange
        const string text = "{\"claims\":[{\"typ\":fortytwo}]}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        Assert.Throws<ArgumentException>(() => AzureAppServiceToken.FromString(token));
    }

    [Fact]
    public void FromString_SingleClaim()
    {
        // Arrange
        const string text = "{\"claims\":[{\"typ\":\"abc\",\"val\":\"ced\"}]}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        var actual = AzureAppServiceToken.FromString(token);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Provider);
        Assert.Null(actual.NameType);
        Assert.Null(actual.RoleType);
        Assert.Single(actual.Claims);
        Assert.Equal("abc", actual.Claims.First().Type);
        Assert.Equal("ced", actual.Claims.First().Value);
    }

    [Fact]
    public void FromString_TwoClaims()
    {
        // Arrange
        const string text = "{\"claims\":[{\"typ\":\"abc\",\"val\":\"ced\"},{\"typ\":\"name\",\"val\":\"Zaphod\"}]}";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        // Act
        var actual = AzureAppServiceToken.FromString(token);

        // Assert
        Assert.NotNull(actual);
        Assert.Null(actual.Provider);
        Assert.Null(actual.NameType);
        Assert.Null(actual.RoleType);
        Assert.Equal(2, actual.Claims.Count());
        Assert.Equal("abc", actual.Claims.First().Type);
        Assert.Equal("Zaphod", actual.Claims.Single(c => c.Type == "name").Value);
    }

    [Fact]
    public void FromString_ComplexClaims()
    {
        // Arrange
        const string token = "ewogICJhdXRoX3R5cCI6ICJhYWQiLAogICJjbGFpbXMiOiBbCiAgICB7CiAgICAgICJ0eXAiOiAiYXVkIiwKICAgICAgInZhbCI6ICJlOWVkNWU1My1iYjI3LTQyMTMtODZlNC04ZDMzNDdiMTZhMzMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImlzcyIsCiAgICAgICJ2YWwiOiAiaHR0cHM6Ly9sb2dpbi5taWNyb3NvZnRvbmxpbmUuY29tL2FiY2RlZmFiLWM3YjQtNDc3My1hODk5LWJhYjJiOTdmNjg2OC92Mi4wIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJpYXQiLAogICAgICAidmFsIjogIjE2MTk3MTIyNDMiCiAgICB9LAogICAgewogICAgICAidHlwIjogIm5iZiIsCiAgICAgICJ2YWwiOiAiMTYxOTcxMjI0MyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiZXhwIiwKICAgICAgInZhbCI6ICIxNjE5NzE2MTQzIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiLAogICAgICAidmFsIjogInRlc3R1c2VyQG91dGxvb2suY29tIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJuYW1lIiwKICAgICAgInZhbCI6ICJUZXN0IFVzZXIiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL29iamVjdGlkZW50aWZpZXIiLAogICAgICAidmFsIjogImZkMTQwMGUxLTRhYjktNDM5Mi1iYjVmLTBhOThlN2MwYmQ3YyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicHJlZmVycmVkX3VzZXJuYW1lIiwKICAgICAgInZhbCI6ICJ0ZXN0dXNlckBvdXRsb29rLmNvbSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicmgiLAogICAgICAidmFsIjogIjAuQVJvQUFZQlZvclRIYzBlb21icXl1WDlvYUZOZTdla251eE5DaHVTTk0wZXhhak1TQU1FLiIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiLAogICAgICAidmFsIjogInZJTFpkN09mYmtjdkl1ZkpEVDRLQVpaczNnWmVyUnRKaWx6WG9CRDR1ZWMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudGlkIiwKICAgICAgInZhbCI6ICJhYmNkZWZhYi1jN2I0LTQ3NzMtYTg5OS1iYWIyYjk3ZjY4NjgiCiAgICB9LAogICAgewogICAgICAidHlwIjogInV0aSIsCiAgICAgICJ2YWwiOiAicmJjMGo5Mzgxa3lleExUcEhFQlFBUSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAidmVyIiwKICAgICAgInZhbCI6ICIyLjAiCiAgICB9CiAgXSwKICAibmFtZV90eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIiwKICAicm9sZV90eXAiOiAiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIgp9";

        // Act
        var actual = AzureAppServiceToken.FromString(token);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("aad", actual.Provider);
        Assert.Equal("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", actual.NameType);
        Assert.Equal("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", actual.RoleType);
        Assert.Equal(14, actual.Claims.Count());
        Assert.Equal("testuser@outlook.com", actual.Claims.Single(c => c.Type.Equals(actual.NameType)).Value);
        Assert.Equal("2.0", actual.Claims.Single(c => c.Type == "ver").Value);
    }
}
