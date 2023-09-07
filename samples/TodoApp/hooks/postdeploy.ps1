<#
    Creates the initial Constants.cs within the TodoApp.Data folder.
#>

$constantsPath = ".\TodoApp.Data\Constants.cs"
if (Test-Path $constantsPath) {
    Write-Host "Constants.cs already exists. Skipping creation."
    System::Exit(0)
}

$azdEnvironment = @{}
azd env get-values | ForEach-Object {
    $key, $value = $_.Split('=')
    $value = $value.Trim('"')
    $azdEnvironment.Add($key, $value)
}

$fileContents = @"
namespace TodoApp.Data
{
    public static class Constants
    {
        /// <summary>
        /// The base URI for the Datasync service.
        /// </summary>
        public static string ServiceUri = "$($azdEnvironment['SERVICE_ENDPOINT'])";
    }
}
"@

Write-Host "Creating Constants.cs at $constantsPath"
$fileContents | Out-File $constantsPath

