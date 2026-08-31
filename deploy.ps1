param (
    [Parameter(Position = 0)]
    [string]$CustomVersion = "",

    [Parameter(Position = 1)]
    [Alias("Image", "App", "Project", "Mode")]
    [ValidateSet("all", "both", "goldex", "goldex-karat", "karat")]
    [string]$Target = "all"
)

$ErrorActionPreference = "Stop"

# Handle case where user passes target as the first positional argument (e.g. .\deploy.ps1 goldex)
if ($CustomVersion -in @("all", "both", "goldex", "goldex-karat", "karat")) {
    $Target = $CustomVersion
    $CustomVersion = ""
}

$Registry = "reg.goldexsoft.ir"
$VersionFile = ".version"

# 1. Version Management
if (-not [string]::IsNullOrWhiteSpace($CustomVersion)) {
    $Version = $CustomVersion
} elseif (Test-Path $VersionFile) {
    $Current = (Get-Content $VersionFile).Trim()
    if ($Current -match '^(\d+\.\d+\.)(\d+)$') {
        $Prefix = $Matches[1]
        $BuildNum = [int]$Matches[2] + 1
        $Version = "$Prefix$BuildNum"
    } else {
        $Version = "1.0.239"
    }
} else {
    $Version = "1.0.239"
}

Set-Content -Path $VersionFile -Value $Version

$BuildGoldEx = $Target.ToLower() -in @("all", "both", "goldex")
$BuildGoldExKarat = $Target.ToLower() -in @("all", "both", "goldex-karat", "karat")

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " GoldEx Docker Build & Push" -ForegroundColor Cyan
Write-Host " Version: $Version" -ForegroundColor Yellow
Write-Host " Target : $Target" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

# Login
Write-Host "`nLogging in to $Registry..." -ForegroundColor Yellow
docker login $Registry

if ($LASTEXITCODE -ne 0) {
    throw "Docker login failed."
}

if ($BuildGoldEx) {
    # Build GoldEx
    Write-Host "`nBuilding GoldEx..." -ForegroundColor Yellow

    docker build `
        --tag "$Registry/goldex:latest" `
        --tag "$Registry/goldex:$Version" `
        --file "src/App/Server/GoldEx.Server/Dockerfile" `
        .

    if ($LASTEXITCODE -ne 0) {
        throw "GoldEx build failed."
    }

    # Push GoldEx
    Write-Host "`nPushing GoldEx..." -ForegroundColor Yellow

    docker push "$Registry/goldex:latest"

    if ($LASTEXITCODE -ne 0) {
        throw "GoldEx latest push failed."
    }

    docker push "$Registry/goldex:$Version"

    if ($LASTEXITCODE -ne 0) {
        throw "GoldEx version push failed."
    }
}

if ($BuildGoldExKarat) {
    # Build GoldEx-Karat
    Write-Host "`nBuilding GoldEx-Karat..." -ForegroundColor Yellow

    docker build `
        --tag "$Registry/goldex-karat:latest" `
        --tag "$Registry/goldex-karat:$Version" `
        --file "src/Calculator/Server/GoldEx.Calculator.Server/Dockerfile" `
        .

    if ($LASTEXITCODE -ne 0) {
        throw "GoldEx-Karat build failed."
    }

    # Push GoldEx-Karat
    Write-Host "`nPushing GoldEx-Karat..." -ForegroundColor Yellow

    docker push "$Registry/goldex-karat:latest"

    if ($LASTEXITCODE -ne 0) {
        throw "GoldEx-Karat latest push failed."
    }

    docker push "$Registry/goldex-karat:$Version"

    if ($LASTEXITCODE -ne 0) {
        throw "GoldEx-Karat version push failed."
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host " Build & Push completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`nImages built/pushed:"
if ($BuildGoldEx) {
    Write-Host "  $Registry/goldex:latest"
    Write-Host "  $Registry/goldex:$Version"
}
if ($BuildGoldExKarat) {
    Write-Host "  $Registry/goldex-karat:latest"
    Write-Host "  $Registry/goldex-karat:$Version"
}

Write-Host "`nRun this on server to deploy:" -ForegroundColor Yellow
Write-Host "/home/user/docker/goldex/refresh-apps.sh $Version" -ForegroundColor Magenta