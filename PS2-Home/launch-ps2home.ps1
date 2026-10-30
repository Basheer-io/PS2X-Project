$ErrorActionPreference = "Stop"
$RootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DotnetRoot = if ($env:DOTNET_ROOT) {
    $env:DOTNET_ROOT
} else {
    Join-Path $env:LOCALAPPDATA "Microsoft\dotnet"
}

$DotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$NeedsDotnet = $true
if ($DotnetCommand) {
    $Version = (& $DotnetCommand.Source --version 2>$null)
    $NeedsDotnet = $Version -notmatch '^10\.'
}

if ($NeedsDotnet) {
    & (Join-Path $RootDir "install-dotnet-windows.ps1")
}

$env:DOTNET_ROOT = $DotnetRoot
$env:PATH = "$DotnetRoot;$env:PATH"

& dotnet run `
    --project (Join-Path $RootDir "src/PS2Home.App/PS2Home.App.csproj") `
    @args

exit $LASTEXITCODE
