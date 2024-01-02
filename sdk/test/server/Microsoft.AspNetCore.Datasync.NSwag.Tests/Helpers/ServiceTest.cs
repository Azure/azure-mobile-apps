using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers;

[ExcludeFromCodeCoverage(Justification = "Test suite")]
public abstract class ServiceTest
{
    protected readonly ServiceApplicationFactory factory;
    protected readonly HttpClient client;

    protected ServiceTest(ServiceApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    protected static string ReadEmbeddedResource(string filename = "swagger.json")
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream s = asm.GetManifestResourceStream(asm.GetName().Name + "." + filename);
        using StreamReader sr = new(s);
        return sr.ReadToEnd().Replace("\r\n", "\n").TrimEnd();
    }

    protected static void WriteExternalFile(string filename, string content)
    {
        string storePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        using StreamWriter outputFile = new(Path.Combine(storePath, filename));
        outputFile.Write(content);
    }
}
