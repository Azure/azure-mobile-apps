"Setup" project is used to make changes in quickstarts source files according to:
- Runtime (DOTNET or NODE)
- Managed SDK version
- WinJS SDK version

Examples:
msbuild Setup.csproj /t:TransformAll /p:Runtime=DOTNET /p:ManagedSDKVersion=2.0.1 /p:StoreSDKVersion=2.0.1 /p:AndroidSDKVersion=2.0.0 /p:JSSDKVersion=2.0.1 /p:WinJSPackageVersion=2.0.1

msbuild Setup.csproj /t:TransformAll /p:Runtime=NODE /p:ManagedSDKVersion=2.0.1 /p:StoreSDKVersion=2.0.1 /p:AndroidSDKVersion=2.0.0 /p:JSSDKVersion=2.0.1 /p:WinJSPackageVersion=2.0.1