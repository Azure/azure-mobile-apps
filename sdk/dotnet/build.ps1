#
# BUILD SCRIPT FOR MANUAL BUILDS
#
# THIS MIRRORS THE BUILD PROCESS DEFINED IN AZURE-PIPELINES.YML
# RUN THIS FROM THE PREVIEW DEVELOPER COMMAND LINE
#

# Restore Workloads
dotnet workload restore --project .\src\Microsoft.Datasync.Client\Microsoft.Datasync.Client.csproj

# Restore NuGet Packages
dotnet restore .\Datasync.Framework.sln

# Build
msbuild -property:Configuration=Release ./Datasync.Framework.sln

# Test
vstest.console --collect:"Code Coverage" .\test\**\bin\Release\**\Microsoft.*.Test.dll

