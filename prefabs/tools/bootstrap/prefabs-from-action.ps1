$artifact = $env:PREFABS_ARTIFACT_NAME
$runid = $env:PREFABS_RUN_ID
$folder = $env:PREFABS_FOLDER
$repo = $env:PREFABS_REPO

if (Test-Path -Path $folder) {
    return "Skipping $folder install as it's already installed. Run '\tools\clean.ps1' to clear the current installation!";
}

Write-Output "Installing $folder..."

mkdir -p $folder

gh run download -R $repo $runid -n $artifact -D $folder

Write-Output "Done."
