$url = $env:STEAMCMD_URL
$folder = $env:STEAMCMD_FOLDER

if (Get-Command "steamcmd.exe" -errorAction SilentlyContinue) {
    return "SteamCMD is already installed and is in PATH, skipping local install and using the globally installed one."
}

Write-Output "Deleting $folder..."
Remove-Item -Force -Recurse "$folder"
Write-Output "Installing $folder..."

mkdir -p $folder
$pastlocation = Get-Location
Set-Location $folder

Invoke-WebRequest $url -OutFile steamcmd.zip
Expand-Archive -LiteralPath steamcmd.zip -DestinationPath .

$env:PATH = "$env:PATH;$(Get-Location)"

Set-Location $pastlocation

Write-Output "Done."
