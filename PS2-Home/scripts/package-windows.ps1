param (
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$RootDir = Split-Path -Parent $PSScriptRoot
$DistDir = Join-Path $RootDir "dist"
$StageDir = Join-Path $DistDir "ps2home-win-x64"
$ArchiveName = "ps2home-win-x64-v$Version.zip"

Write-Host "=== Building PS2 Home v$Version for Windows (x64) ===" -ForegroundColor Cyan

# Clean staging area
if (Test-Path $DistDir) {
    Remove-Item -Recurse -Force $DistDir
}
New-Item -ItemType Directory -Force -Path $StageDir | Out-Null

# 1. Build C++ Native Diagnostics Helper
Write-Host "--- Building C++ Native Helper (ps2home-diag.exe) ---" -ForegroundColor Yellow
cmake -B "$RootDir/native/PS2Home.Native/build" -S "$RootDir/native/PS2Home.Native" -DCMAKE_BUILD_TYPE=Release
cmake --build "$RootDir/native/PS2Home.Native/build" --config Release

# 2. Publish Self-Contained .NET 10 App
Write-Host "--- Publishing Self-Contained .NET 10 Application ---" -ForegroundColor Yellow
dotnet publish "$RootDir/src/PS2Home.App/PS2Home.App.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $StageDir

# 3. Bundle Native Helper & Docs
Write-Host "--- Bundling Native Helper & Docs ---" -ForegroundColor Yellow
Copy-Item "$RootDir/native/PS2Home.Native/build/Release/ps2home-diag.exe" -Destination $StageDir -ErrorAction SilentlyContinue
if (-not (Test-Path "$StageDir/ps2home-diag.exe")) {
    Copy-Item "$RootDir/native/PS2Home.Native/build/ps2home-diag.exe" -Destination $StageDir -ErrorAction SilentlyContinue
}

Copy-Item "$RootDir/README.md" -Destination $StageDir
Copy-Item "$RootDir/LICENSE" -Destination $StageDir
Copy-Item "$RootDir/THIRD-PARTY-NOTICES.md" -Destination $StageDir

# 4. Create ZIP Archive
Write-Host "--- Packaging ZIP Archive ---" -ForegroundColor Yellow
$ArchivePath = Join-Path $DistDir $ArchiveName
Compress-Archive -Path "$StageDir/*" -DestinationPath $ArchivePath -CompressionLevel Optimal

# 5. Compute SHA-256 Checksum
$Hash = (Get-FileHash -Path $ArchivePath -Algorithm SHA256).Hash
Set-Content -Path "$ArchivePath.sha256" -Value "$Hash  $ArchiveName"

Write-Host "=== Release package successfully created! ===" -ForegroundColor Green
Write-Host "Package:  $ArchivePath"
Write-Host "Checksum: $Hash"
