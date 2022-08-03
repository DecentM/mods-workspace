$unitysetup = $env:UNITYSETUP_FILENAME
$url = $env:UNITYSETUP_URL
$folder = $env:MONO_FOLDER

Write-Output "Installing Mono..."

if (!(Test-Path -Path $folder)) {
    mkdir -p $folder
}

$pastlocation = Get-Location
Set-Location $folder

if (Test-Path -Path "$unitysetup") {
    Write-Output "Found existing $unitysetup"
} else {
    Write-Output "Downloading $unitysetup..."
    Invoke-WebRequest $url -OutFile "$unitysetup"
}

if (Test-Path -Path "MonoExtract") {
    Remove-Item -Force -Recurse .\MonoExtract
}

$monopath = $env:UNITYSETUP_MONO_PATH

7z x "$unitysetup" "$monopath" -oMonoExtract

Set-Location $pastlocation

Write-Output "Done."
