$ErrorActionPreference = "Stop"

$DotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($DotnetCommand) {
    $Version = (& $DotnetCommand.Source --version 2>$null)
    if ($Version -match '^10\.') {
        Write-Host "A .NET 10 SDK is already installed."
        exit 0
    }
}

Write-Host "PS2 Home needs the .NET 10 SDK to run from source."
$Answer = Read-Host "Install it for your user from Microsoft's official installer? [Y/n]"
if ($Answer -and $Answer -notmatch '^[Yy]$') {
    throw "Installation cancelled."
}

$InstallScript = Join-Path ([System.IO.Path]::GetTempPath()) "dotnet-install-$([guid]::NewGuid()).ps1"
$DotnetDir = if ($env:DOTNET_ROOT) {
    $env:DOTNET_ROOT
} else {
    Join-Path $env:LOCALAPPDATA "Microsoft\dotnet"
}

try {
    Invoke-WebRequest `
        -Uri "https://dot.net/v1/dotnet-install.ps1" `
        -OutFile $InstallScript `
        -UseBasicParsing

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $InstallScript `
        -Channel "10.0" `
        -InstallDir $DotnetDir `
        -NoPath

    if ($LASTEXITCODE -ne 0) {
        throw ".NET installer exited with code $LASTEXITCODE."
    }
} finally {
    Remove-Item -LiteralPath $InstallScript -Force -ErrorAction SilentlyContinue
}

Write-Host "Installed .NET 10 SDK in $DotnetDir."
