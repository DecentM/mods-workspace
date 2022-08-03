$folder = $env:STEAM_GAME_FOLDER
$melonfolder = $env:MELONLOADER_FOLDER

Write-Output "Installing Melonloader into ChilloutVR..."

Copy-Item -Force -Recurse .\$melonfolder\MelonLoader.x64\* .\$folder\
# Copy-Item -Force .\MelonLoader\MelonLoader.x64\version.dll .\ChilloutVR\version.dll
if (!(Test-Path -Path $folder)) {
    Write-Output "Mods folder doesn't exist, creating..."
    mkdir -p .\ChilloutVR\Mods
}

# This folder doesn't exist during a ci build
if (Test-Path -Path "Mono") {
    Write-Output "Converting retail build into debug build..."

    Copy-Item -Force -Recurse "Mono\MonoExtract\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\Data\*" ".\chilloutvr\ChilloutVR_Data\"
    Copy-Item -Force ".\Mono\MonoExtract\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\UnityPlayer.dll" ".\chilloutvr\UnityPlayer.dll"
    Copy-Item -Force ".\Mono\MonoExtract\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\WindowsPlayer.exe" ".\chilloutvr\ChilloutVR.exe"
    Copy-Item -Force ".\Mono\MonoExtract\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\WinPixEventRuntime.dll" ".\chilloutvr\WinPixEventRuntime.dll"
    Copy-Item -Force ".\Mono\MonoExtract\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono\MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll" ".\chilloutvr\MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll"
}

Write-Output "Done."
