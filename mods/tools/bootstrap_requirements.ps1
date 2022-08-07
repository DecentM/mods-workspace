$env:PATH += ";C:\Program Files\7-Zip"

if (!(Get-Command "7z.exe" -errorAction SilentlyContinue)) {
    Write-Error "7z.exe not found in path. Install 7zip from 7zip.org, reboot, then try again. If your install destination is non-default, you need to add the install directory into the PATH environment variable."
    exit 1001;
}

if (!(Get-Command "git.exe" -errorAction SilentlyContinue)) {
    Write-Error "git.exe not found in path. Install Git from git-scm.com, reboot, then try again."
    exit 1002;
}
