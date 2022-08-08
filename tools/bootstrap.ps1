$pastlocation = Get-Location

Set-Location ..\mods
.\tools\bootstrap_all.ps1

Set-Location $pastlocation
