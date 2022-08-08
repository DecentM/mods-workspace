$url = $env:NUGET_URL
$folder = $env:NUGET_FOLDER

if (Get-Command "nuget.exe" -errorAction SilentlyContinue) {
    return "NuGet is already installed and is in PATH, skipping local install and using the globally installed one."
}

Write-Output "Deleting $folder..."
Remove-Item -Force -Recurse "$folder"
Write-Output "Installing $folder..."

mkdir -p $folder
$pastlocation = Get-Location
Set-Location $folder

Invoke-WebRequest $url -OutFile nuget.exe

$env:PATH = "$env:PATH;$(Get-Location)"

Set-Location $pastlocation

Write-Output "Done."
