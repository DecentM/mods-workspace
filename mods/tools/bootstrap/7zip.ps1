return "7-Zip should be installed system-wide. 7zr cannot extract the Unity Installer. Skipping...";

if (Test-Path -Path $env:7ZIP_FOLDER) {
    return "Skipping 7-Zip install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing 7-Zip..."

mkdir -p $env:7ZIP_FOLDER
$pastlocation = Get-Location
Set-Location $env:7ZIP_FOLDER

Invoke-WebRequest $env:7ZIP_URL -OutFile "7z.exe"
# .\7zip-setup.exe /S /D=".\7-Zip"

Set-Location $pastlocation

Write-Output "Done."
