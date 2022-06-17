#
# BUILD SCRIPT FOR MANUAL BUILDS
#
# THIS MIRRORS THE BUILD PROCESS DEFINED IN AZURE-PIPELINES.YML
# RUN THIS FROM THE PREVIEW DEVELOPER COMMAND LINE
#

# Restore Workloads
dotnet workload restore --project .\src\Microsoft.Datasync.Client\Microsoft.Datasync.Client.csproj

# Build
msbuild ./Datasync.Framework.sln

# Test
vstest.console --collect:"Code Coverage" **\Microsoft.*.Test.dll

