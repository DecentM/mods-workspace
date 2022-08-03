$url = $env:STEAMCMD_URL
$folder = $env:STEAMCMD_FOLDER

if (Test-Path -Path $folder) {
    return "Skipping SteamCMD install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing $folder..."

mkdir -p $folder
$pastlocation = Get-Location
Set-Location $folder

Invoke-WebRequest $url -OutFile steamcmd.zip
Expand-Archive -LiteralPath steamcmd.zip -DestinationPath steamcmd

Set-Location $pastlocation

Write-Output "Done."
