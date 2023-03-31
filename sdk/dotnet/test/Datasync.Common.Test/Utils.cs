// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Text;

namespace Datasync.Common.Test;

[ExcludeFromCodeCoverage]
public static class Utils
{
    /// <summary>
    /// Converts an index into an ID for the Movies controller.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static string GetMovieId(int index) => string.Format("id-{0:000}", index);

    /// <summary>
    /// Gets the authentication token based on the userId
    /// </summary>
    /// <remarks>
    /// The <c>X-MS-CLIENT-PRINCIPAL</c> is just Base-64 encoded JSON.
    /// </remarks>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string GetAuthToken(string userId)
    {
        // The tokens are Base64 encoded JSON objects.
        var token = new AzureAppServiceToken()
        {
            Provider = "aad",
            NameType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            RoleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
        List<AzureAppServiceToken.UserClaim> claims = new()
        {
            new AzureAppServiceToken.UserClaim { Type = "aud", Value = "e9ed5e53-bb27-4213-86e4-8d3347b16a33" },
            new AzureAppServiceToken.UserClaim { Type = "iss", Value = "https://login.microsoftonline.com/abcdefab-c7b4-4773-a899-bab2b97f6868/v2.0" },
            new AzureAppServiceToken.UserClaim { Type = "iat", Value = "1619712243" },
            new AzureAppServiceToken.UserClaim { Type = "nbf", Value = "1619712243" },
            new AzureAppServiceToken.UserClaim { Type = "exp", Value = "1619716143" },
            new AzureAppServiceToken.UserClaim { Type = "rh", Value = "0.ARoAAYBVorTHc0eombqyuX9oaFNe7eknuxNChuSNM0exajMSAME." },
            new AzureAppServiceToken.UserClaim { Type = "name", Value = "Test User" },
            new AzureAppServiceToken.UserClaim { Type = "http://schemas.microsoft.com/identity/claims/objectidentifier", Value = "fd1400e1-4ab9-4392-bb5f-0a98e7c0bd7c" },
            new AzureAppServiceToken.UserClaim { Type = "preferred_username", Value = "testuser@outlook.com" },
            new AzureAppServiceToken.UserClaim { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", Value = "vILZd7OfbkcvIufJDT4KAZZs3gZerRtJilzXoBD4uec" },
            new AzureAppServiceToken.UserClaim { Type = "http://schemas.microsoft.com/identity/claims/tenantid", Value = "abcdefab-c7b4-4773-a899-bab2b97f6868" },
            new AzureAppServiceToken.UserClaim { Type = "uti", Value = "rbc0j9381kyexLTpHEBQAQ" },
            new AzureAppServiceToken.UserClaim { Type = "ver", Value = "2.0" }
        };
        if (userId == "success")
        {
            claims.Add(new AzureAppServiceToken.UserClaim { Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", Value = "testuser@outlook.com" });
        }
        token.Claims = claims;

        var json = JsonConvert.SerializeObject(token);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
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
