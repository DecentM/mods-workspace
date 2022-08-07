$url = $env:NUGET_URL
$folder = $env:NUGET_FOLDER

if (Test-Path -Path $folder) {
    return "Skipping NuGet install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing NuGet..."

mkdir -p $folder
$pastlocation = Get-Location
Set-Location $folder

Invoke-WebRequest $url -OutFile nuget.exe

Set-Location $pastlocation

Write-Output "Done."
