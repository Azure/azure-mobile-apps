///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetVersion = Argument("nugetVersion", EnvironmentVariable("NUGET_VERSION") ?? "1.0.0");
var baseVersion = nugetVersion.Contains("-")
    ? nugetVersion.Substring(0, nugetVersion.IndexOf("-"))
    : nugetVersion;
var outputDirectory = Argument("artifactsPath", "../../output");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Build").Does(() =>
{
    Information("Building with NuGet version: {0}", nugetVersion);
    Information("Building with assembly version: {0}", baseVersion);

    MSBuild("./Datasync.Framework.sln", c => c
        .SetConfiguration(configuration)
        .EnableBinaryLogger($"./output/build.binlog")
        .WithRestore()
        .WithTarget("Build")
        .WithProperty("PackageOutputPath", MakeAbsolute((DirectoryPath)"./output").FullPath)
        .WithProperty("PackageVersion", nugetVersion)
        .WithProperty("Version", baseVersion));
});

Task("Test").IsDependentOn("Build").Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
        ResultsDirectory = $"./output/unittests-results"
    };

    var failCount = 0;
    var projectFiles = GetFiles("./test/Microsoft.*/Microsoft.*.csproj");
    foreach(var file in projectFiles)
    {
        settings.Logger = "trx;LogFileName=" + file.GetFilenameWithoutExtension() + "-Results.trx";
        try
        {
            DotNetCoreTest(file.FullPath, settings);
        }
        catch
        {
            failCount++;
        }
    }

    if (failCount > 0)
        throw new Exception($"There were {failCount} test failures.");
});

Task("Pack").IsDependentOn("Build").Does(() => {
    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
        IncludeSource = true,
        IncludeSymbols = true,
        OutputDirectory = "./output"
    };

    var projectFiles = GetFiles("./src/**/*.csproj");
    foreach (var file in profileFiles) 
    {
        DotNetCorePack(file.FullPath, settings);
    }
});

Task("Copy").IsDependentOn("Pack").IsDependentOn("Test").Does(() => {
    CopyFiles("./output/**", (DirectoryPath)"../../output/");
});

///////////////////////////////////////////////////////////////////////////////
// ENTRYPOINTS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .IsDependentOn("Copy");

Task("ci")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .IsDependentOn("Copy");

RunTarget(target);