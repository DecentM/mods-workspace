$folder = $env:PREFABS_FOLDER
$url = $env:PREFABS_URL

if (Test-Path -Path $folder) {
    return "Skipping $folder install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing $folder..."

mkdir -p $folder

Invoke-WebRequest $url -OutFile $folder/

Write-Output "Done."
