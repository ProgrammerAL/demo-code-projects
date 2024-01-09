$publishedAppsDirectory = "$PSScriptRoot/published-apps"

if (Test-Path -Path $publishedAppsDirectory) {
    Remove-Item -Path $publishedAppsDirectory -Recurse
}

New-Item -Path $publishedAppsDirectory -ItemType Directory
New-Item -Path "$publishedAppsDirectory/webapp" -ItemType Directory

#Sanity check, make sure we're running from the proper loction
Set-Location "$PSScriptRoot/FeedbackFunctionsApp"
& "dotnet" publish -c Release
Compress-Archive -Path "$PSScriptRoot/FeedbackFunctionsApp/bin/Release/net8.0/publish/*" -DestinationPath "$publishedAppsDirectory/functions.zip"

Set-Location "$PSScriptRoot/FeedbackWebApp"
& "dotnet" publish -c Release
Copy-Item -Path "$PSScriptRoot/FeedbackWebApp/bin/Release/net8.0/publish/wwwroot/*" -Destination "$publishedAppsDirectory/webapp" -Recurse

#To make the upload quicker for demos, delete the compressed files. We don't need them for the demo
# In real applications, you should keep these compressed files
Remove-Item -Path "$publishedAppsDirectory/webapp/_framework/*.gz"
Remove-Item -Path "$publishedAppsDirectory/webapp/_framework/*.br"
