// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Swashbuckle.Test.Service;
using Microsoft.AspNetCore.TestHost;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Test;

public class SwaggerGen_Tests
{
    private readonly TestServer server = SwaggerServer.CreateTestServer();

    private static string ReadExternalFile(string filename)
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream s = asm.GetManifestResourceStream(asm.GetName().Name + "." + filename)!;
        using StreamReader sr = new(s);
        return sr.ReadToEnd();
    }

    [Fact]
    public void DocumentFilter_ReadsAllControllers()
    {
        Assert.NotNull(server);

        var controllers = DatasyncDocumentFilter.GetAllTableControllers().Select(m => m.Name).ToList();

        // There should be two controllers
        Assert.Equal(2, controllers.Count);
        Assert.Contains("KitchenSinkController", controllers);
        Assert.Contains("TodoItemController", controllers);
    }

    [Fact]
    public async Task SwaggerGen_GeneratesSwagger()
    {
        var swaggerDoc = await server.SendRequest(HttpMethod.Get, "swagger/v1/swagger.json");
        Assert.NotNull(swaggerDoc);
        Assert.True(swaggerDoc!.IsSuccessStatusCode);

        var expectedContent = ReadExternalFile("swagger.json").Replace("\r\n", "\n").TrimEnd();
        var actualContent = await swaggerDoc!.Content.ReadAsStringAsync();
        Assert.Equal(expectedContent, actualContent.Replace("\r\n", "\n").TrimEnd());
    }
}
