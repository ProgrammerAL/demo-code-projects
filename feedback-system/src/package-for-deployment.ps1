$publishedAppsDirectory = "$PSScriptRoot/published-apps"

if (Test-Path -Path $publishedAppsDirectory) {
    Remove-Item -Path $publishedAppsDirectory -Recurse
}

New-Item -Path $publishedAppsDirectory -ItemType Directory

#Sanity check, make sure we're running from the proper loction
Set-Location "$PSScriptRoot/FeedbackFunctionsApp"
& "dotnet" publish -c Release
Compress-Archive -Path "$PSScriptRoot/FeedbackFunctionsApp/bin/Release/net8.0/publish/*" -DestinationPath "$publishedAppsDirectory/functions.zip"

Set-Location "$PSScriptRoot/FeedbackWebApp"
& "dotnet" publish -c Release
Compress-Archive -Path "$PSScriptRoot/FeedbackWebApp/bin/Release/net8.0/*" -DestinationPath "$publishedAppsDirectory/webapp.zip"
