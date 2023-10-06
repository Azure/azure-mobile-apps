// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.NSwag.Test.Service;
using Microsoft.AspNetCore.TestHost;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.NSwag.Test;

[ExcludeFromCodeCoverage]
public class NSwag_Tests
{
    private readonly TestServer server = NSwagServer.CreateTestServer();

    private static string ReadExternalFile(string filename)
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream s = asm.GetManifestResourceStream(asm.GetName().Name + "." + filename)!;
        using StreamReader sr = new(s);
        return sr.ReadToEnd();
    }

    private static void WriteExternalFile(string filename, string content)
    {
        var storePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        using StreamWriter outputFile = new(Path.Combine(storePath, filename));
        outputFile.Write(content);
    }

    [Fact]
    public async Task NSwag_GeneratesSwagger()
    {
        var swaggerDoc = await server.SendRequest(HttpMethod.Get, "swagger/v1/swagger.json");
        Assert.NotNull(swaggerDoc);
        Assert.True(swaggerDoc!.IsSuccessStatusCode);

        var expectedContent = ReadExternalFile("swagger.json").Replace("\r\n", "\n").TrimEnd();
        var actualContent = (await swaggerDoc!.Content.ReadAsStringAsync()).Replace("\r\n", "\n").TrimEnd();
        if (!expectedContent.Equals(actualContent))
        {
            WriteExternalFile("swagger.json.out", actualContent);
        }
        Assert.Equal(expectedContent, actualContent);
    }
}