// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Datasync.Common.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class Utils
    {
        private const string ValidAadToken = "ewogICJhdXRoX3R5cCI6ICJhYWQiLAogICJjbGFpbXMiOiBbCiAgICB7CiAgICAgICJ0eXAiOiAiYXVkIiwKICAgICAgInZhbCI6ICJlOWVkNWU1My1iYjI3LTQyMTMtODZlNC04ZDMzNDdiMTZhMzMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImlzcyIsCiAgICAgICJ2YWwiOiAiaHR0cHM6Ly9sb2dpbi5taWNyb3NvZnRvbmxpbmUuY29tL2FiY2RlZmFiLWM3YjQtNDc3My1hODk5LWJhYjJiOTdmNjg2OC92Mi4wIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJpYXQiLAogICAgICAidmFsIjogIjE2MTk3MTIyNDMiCiAgICB9LAogICAgewogICAgICAidHlwIjogIm5iZiIsCiAgICAgICJ2YWwiOiAiMTYxOTcxMjI0MyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiZXhwIiwKICAgICAgInZhbCI6ICIxNjE5NzE2MTQzIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiLAogICAgICAidmFsIjogInRlc3R1c2VyQG91dGxvb2suY29tIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJuYW1lIiwKICAgICAgInZhbCI6ICJUZXN0IFVzZXIiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL29iamVjdGlkZW50aWZpZXIiLAogICAgICAidmFsIjogImZkMTQwMGUxLTRhYjktNDM5Mi1iYjVmLTBhOThlN2MwYmQ3YyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicHJlZmVycmVkX3VzZXJuYW1lIiwKICAgICAgInZhbCI6ICJ0ZXN0dXNlckBvdXRsb29rLmNvbSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicmgiLAogICAgICAidmFsIjogIjAuQVJvQUFZQlZvclRIYzBlb21icXl1WDlvYUZOZTdla251eE5DaHVTTk0wZXhhak1TQU1FLiIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiLAogICAgICAidmFsIjogInZJTFpkN09mYmtjdkl1ZkpEVDRLQVpaczNnWmVyUnRKaWx6WG9CRDR1ZWMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudGlkIiwKICAgICAgInZhbCI6ICJhYmNkZWZhYi1jN2I0LTQ3NzMtYTg5OS1iYWIyYjk3ZjY4NjgiCiAgICB9LAogICAgewogICAgICAidHlwIjogInV0aSIsCiAgICAgICJ2YWwiOiAicmJjMGo5Mzgxa3lleExUcEhFQlFBUSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAidmVyIiwKICAgICAgInZhbCI6ICIyLjAiCiAgICB9CiAgXSwKICAibmFtZV90eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIiwKICAicm9sZV90eXAiOiAiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIgp9";
        private const string InvalidAadToken = "ewogICJhdXRoX3R5cCI6ICJhYWQiLAogICJjbGFpbXMiOiBbCiAgICB7CiAgICAgICJ0eXAiOiAiYXVkIiwKICAgICAgInZhbCI6ICJlOWVkNWU1My1iYjI3LTQyMTMtODZlNC04ZDMzNDdiMTZhMzMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImlzcyIsCiAgICAgICJ2YWwiOiAiaHR0cHM6Ly9sb2dpbi5taWNyb3NvZnRvbmxpbmUuY29tL2FiY2RlZmFiLWM3YjQtNDc3My1hODk5LWJhYjJiOTdmNjg2OC92Mi4wIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJpYXQiLAogICAgICAidmFsIjogIjE2MTk3MTIyNDMiCiAgICB9LAogICAgewogICAgICAidHlwIjogIm5iZiIsCiAgICAgICJ2YWwiOiAiMTYxOTcxMjI0MyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiZXhwIiwKICAgICAgInZhbCI6ICIxNjE5NzE2MTQzIgogICAgfSwKICAgIHsKICAgICAgInR5cCI6ICJuYW1lIiwKICAgICAgInZhbCI6ICJUZXN0IFVzZXIiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL29iamVjdGlkZW50aWZpZXIiLAogICAgICAidmFsIjogImZkMTQwMGUxLTRhYjktNDM5Mi1iYjVmLTBhOThlN2MwYmQ3YyIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicHJlZmVycmVkX3VzZXJuYW1lIiwKICAgICAgInZhbCI6ICJ0ZXN0dXNlckBvdXRsb29rLmNvbSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAicmgiLAogICAgICAidmFsIjogIjAuQVJvQUFZQlZvclRIYzBlb21icXl1WDlvYUZOZTdla251eE5DaHVTTk0wZXhhak1TQU1FLiIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiLAogICAgICAidmFsIjogInZJTFpkN09mYmtjdkl1ZkpEVDRLQVpaczNnWmVyUnRKaWx6WG9CRDR1ZWMiCiAgICB9LAogICAgewogICAgICAidHlwIjogImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudGlkIiwKICAgICAgInZhbCI6ICJhYmNkZWZhYi1jN2I0LTQ3NzMtYTg5OS1iYWIyYjk3ZjY4NjgiCiAgICB9LAogICAgewogICAgICAidHlwIjogInV0aSIsCiAgICAgICJ2YWwiOiAicmJjMGo5Mzgxa3lleExUcEhFQlFBUSIKICAgIH0sCiAgICB7CiAgICAgICJ0eXAiOiAidmVyIiwKICAgICAgInZhbCI6ICIyLjAiCiAgICB9CiAgXSwKICAibmFtZV90eXAiOiAiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIiwKICAicm9sZV90eXAiOiAiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIgp9";

        /// <summary>
        /// Converts an index into an ID for the Movies controller.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetMovieId(int index) => string.Format("id-{0:000}", index);

        /// <summary>
        /// Gets the authentication token based on the userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetAuthToken(string userId)
        {
            return userId == "success" ? ValidAadToken : InvalidAadToken;
            // The tokens are Base64 encoded JSON objects.
            //var token = new AzureAppServiceToken()
            //{
            //    Provider = "aad",
            //    NameType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            //    RoleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            //};
            //List<AzureAppServiceToken.UserClaim> claims = new();
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "aud", Value = "e9ed5e53-bb27-4213-86e4-8d3347b16a33" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "iss", Value = "https://login.microsoftonline.com/abcdefab-c7b4-4773-a899-bab2b97f6868/v2.0" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "iat", Value = "1619712243" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "nbf", Value = "1619712243" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "exp", Value = "1619716143" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "rh", Value = "0.ARoAAYBVorTHc0eombqyuX9oaFNe7eknuxNChuSNM0exajMSAME." });
            //if (userId == "success")
            //{
            //    claims.Add(new AzureAppServiceToken.UserClaim { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", Value = "testuser@outlook.com" });
            //}
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "name", Value = "Test User" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "http://schemas.microsoft.com/identity/claims/objectidentifier", Value = "fd1400e1-4ab9-4392-bb5f-0a98e7c0bd7c" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "preferred_username", Value = "testuser@outlook.com" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Value = "vILZd7OfbkcvIufJDT4KAZZs3gZerRtJilzXoBD4uec" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "http://schemas.microsoft.com/identity/claims/tenantid", Value = "abcdefab-c7b4-4773-a899-bab2b97f6868" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "uti", Value = "rbc0j9381kyexLTpHEBQAQ" });
            //claims.Add(new AzureAppServiceToken.UserClaim { Type = "ver", Value = "2.0" });

            //token.Claims = claims;

            //var json = JsonSerializer.Serialize<AzureAppServiceToken>(token);
            //var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            //return b64;
        }

        /// <summary>
        /// Adds the authentication headers to the provided headers.
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="userId"></param>
        public static void AddAuthHeaders(Dictionary<string, string> headers, string userId)
        {
            if (userId != null)
            {
                headers.Add("X-MS-CLIENT-PRINCIPAL", Utils.GetAuthToken(userId));
                headers.Add("X-MS-CLIENT-PRINCIPAL-IDP", "aad");
                headers.Add("X-MS-CLIENT-PRINCIPAL-NAME", "testuser@outlook.com");
            }
        }

        /// <summary>
        /// A basic AsyncEnumerator.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<int> RangeAsync(int start, int count, int ms = 1)
        {
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(ms).ConfigureAwait(false);
                yield return start + i;
            }
        }

        /// <summary>
        /// An alternate basic AsyncEnumerator that throws half way through.
        /// </summary>
        /// <returns></returns>
        public static async IAsyncEnumerable<int> ThrowAsync()
        {
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(1).ConfigureAwait(false);
                if (i < 10)
                {
                    yield return i;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
