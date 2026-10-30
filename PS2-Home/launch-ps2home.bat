@echo off
setlocal

set "ROOT_DIR=%~dp0"
if not defined DOTNET_ROOT (
    where dotnet >nul 2>&1
    if errorlevel 1 (
        echo dotnet was not found. Install the .NET 10 SDK, then run this launcher again.
        exit /b 1
    )
)

dotnet run --project "%ROOT_DIR%src\PS2Home.App\PS2Home.App.csproj" %*
exit /b %errorlevel%
