$env:STEAM_GAME_ID = 661130
$env:STEAM_GAME_FOLDER = "chilloutvr"

$env:MELONLOADER_REPO = "LavaGang/MelonLoader"
$env:MELONLOADER_FOLDER = "melonloader"
$env:MELONLOADER_VERSION = "v0.5.4"

$env:UNITYSETUP_FILENAME = "UnitySetup64-2019.4.28f1.exe"
$env:UNITYSETUP_BUCKET = "1381962e9d08"
$env:UNITYSETUP_URL = "https://download.unity3d.com/download_unity/$env:UNITYSETUP_BUCKET/Windows64EditorInstaller/$env:UNITYSETUP_FILENAME"
$env:UNITYSETUP_MONO_PATH = "Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono"
$env:MONO_FOLDER = "mono"

$env:NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$env:NUGET_FOLDER = "nuget"

$env:STEAMCMD_URL = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip"
$env:STEAMCMD_FOLDER = "steamcmd"

# We can't prompt for anything in the CI, but that should have set STEAM_USER and STEAM_PASSWORD already
if (!($env:CI -eq "true")) {
    $c = $host.ui.PromptForCredential("Steam Credentials", "Log into a Steam account that owns game $env:STEAM_GAME_ID, to install into $env:STEAM_GAME_FOLDER.", "", "")
    $env:STEAM_USER = $c.UserName
    $env:STEAM_PASSWORD = (New-Object PSCredential "user", $c.Password).GetNetworkCredential().Password
}
