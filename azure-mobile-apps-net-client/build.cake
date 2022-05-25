///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetVersion = Argument("nugetVersion", EnvironmentVariable("NUGET_VERSION") ?? "1.0.0");
var baseVersion = nugetVersion.Contains("-")
    ? nugetVersion.Substring(0, nugetVersion.IndexOf("-"))
    : nugetVersion;

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
{
    Information("Building with NuGet version: {0}", nugetVersion);
    Information("Building with assembly version: {0}", baseVersion);

    MSBuild("./Microsoft.Azure.Mobile.Client.sln", c => c
        .SetConfiguration(configuration)
        .EnableBinaryLogger("./output/build.binlog")
        .WithRestore()
        .WithTarget("Build")
        .WithProperty("PackageOutputPath", MakeAbsolute((DirectoryPath)"./output/").FullPath)
        .WithProperty("PackageVersion", nugetVersion)
        .WithProperty("Version", baseVersion));
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = "Release",
        NoBuild = true,
        NoRestore = true,
        ResultsDirectory = "./output/unittests-results"
    };

    var failCount = 0;

    var projectFiles = GetFiles("./unittests/**/*.csproj");
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

///////////////////////////////////////////////////////////////////////////////
// ENTRYPOINTS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("ci")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

RunTarget(target);
