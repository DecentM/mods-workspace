return "7-Zip should be installed system-wide. 7zr cannot extract the Unity Installer. Skipping...";

if (Get-Command "7z.exe" -errorAction SilentlyContinue) {
    return "7-Zip is already installed and is in PATH, skipping local install and using the globally installed one."
}

Write-Output "Deleting $folder..."
Remove-Item -Force -Recurse "$folder"
Write-Output "Installing 7-Zip..."

mkdir -p $env:7ZIP_FOLDER
$pastlocation = Get-Location
Set-Location $env:7ZIP_FOLDER

Invoke-WebRequest $env:7ZIP_URL -OutFile "7z.exe"
# .\7zip-setup.exe /S /D=".\7-Zip"

Set-Location $pastlocation
$env:PATH = "$env:PATH;.\$folder\"

Write-Output "Done."
