$ErrorActionPreference = "Stop"
$RootDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet was not found. Install the .NET 10 SDK, then run this launcher again."
}

& dotnet run `
    --project (Join-Path $RootDir "src/PS2Home.App/PS2Home.App.csproj") `
    @args

exit $LASTEXITCODE
