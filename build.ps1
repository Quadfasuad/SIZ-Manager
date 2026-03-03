# SIZ Manager — скрипт сборки и компиляции инсталлятора
# Использование: .\build.ps1
# Версия берётся из SizManager/SizManager.csproj — менять только там

$ErrorActionPreference = "Stop"
$IsccPath = "H:\Program\Inno Setup 6\ISCC.exe"

# Читаем версию из .csproj
$csproj = [xml](Get-Content "$PSScriptRoot\SizManager\SizManager.csproj")
$version = $csproj.Project.PropertyGroup.Version
Write-Host "==> Версия: $version" -ForegroundColor Cyan

# Publish
Write-Host "==> dotnet publish..." -ForegroundColor Cyan
dotnet publish "$PSScriptRoot\SizManager\SizManager.csproj" `
    -c Release -r win-x64 --self-contained --nologo
if ($LASTEXITCODE -ne 0) { exit 1 }

# Компиляция инсталлятора
Write-Host "==> Компиляция инсталлятора..." -ForegroundColor Cyan
& $IsccPath /DMyAppVersion=$version "$PSScriptRoot\Installer\SizManager.iss"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "==> Готово: Installer\Output\SizManager_Setup_${version}_x64.exe" -ForegroundColor Green
