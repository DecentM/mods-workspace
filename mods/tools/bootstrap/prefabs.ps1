$version = $env:PREFABS_VERSION
$filename = $env:PREFABS_FILENAME
$url = $env:PREFABS_URL
$folder = $env:PREFABS_FOLDER

if (Test-Path -Path $folder) {
    return "Skipping DecentM.Prefabs install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing $folder..."

mkdir -p $folder
$pastlocation = Get-Location
Set-Location $folder

Invoke-WebRequest $url -OutFile "$filename"

Set-Location $pastlocation

Write-Output "Done."
