$delfolders = ".\bin", ".\chilloutvr", ".\melonloader", ".\steamcmd", ".\nuget", ".\prefabs"

foreach ($delfolder in $delfolders) {
    if (Test-Path -Path $delfolder) {
        Write-Output "Recursively deleting: $delfolder"
        Remove-Item -Force -Recurse $delfolder
    }
}

Write-Output "Done. Run .\tools\bootstrap_all.ps1 to now do a clean installation!"
