$folder = $env:MELONLOADER_FOLDER
$repo = $env:MELONLOADER_REPO
$version = $env:MELONLOADER_VERSION

if (Test-Path -Path $folder) {
    return "Skipping Melonloader install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing Melonloader..."

mkdir -p $folder
$pastlocation = Get-Location
Set-Location $folder

gh release -R $repo download $version -p "MelonLoader.x64.zip"
Expand-Archive -LiteralPath MelonLoader.x64.zip -DestinationPath MelonLoader.x64

Set-Location $pastlocation

Write-Output "Done."
