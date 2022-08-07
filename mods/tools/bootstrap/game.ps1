$gameid = $env:STEAM_GAME_ID
$folder = $env:STEAM_GAME_FOLDER

if ($null -eq $env:STEAM_USER) {
    Write-Output "The STEAM_USER environment variable is not set. Cannot download the game."
    exit 1
}

if ($null -eq $env:STEAM_PASSWORD) {
    Write-Output "The STEAM_PASSWORD environment variable is not set. Cannot download the game."
    exit 2
}

Write-Output "Installing ChilloutVR..."

steamcmd `
    +@sSteamCmdForcePlatformType windows `
    +force_install_dir ../../$folder `
    +login $env:STEAM_USER "$env:STEAM_PASSWORD" `
    +app_update $gameid validate `
    +quit `
    || & {
        if ($global:LASTEXITCODE -eq 7) {
            "SteamCMD exited with exit code $global:LASTEXITCODE, but we don't care about it."; $global:LASTEXITCODE = 0
        }
     }

Write-Output "Done."

