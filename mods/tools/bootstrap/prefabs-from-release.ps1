$folder = $env:PREFABS_FOLDER
$url = $env:PREFABS_URL
$filename = $env:PREFABS_FILENAME

if (Test-Path -Path $folder) {
    return "Skipping $folder install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing $folder..."

mkdir -p $folder

Invoke-WebRequest $url -OutFile "$folder/$filename"
Expand-Archive -LiteralPath "$folder/$filename" -DestinationPath "$folder"
Remove-Item -Force "$folder/$filename"

Write-Output "Done."
