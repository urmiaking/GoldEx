param (
    [Parameter(Position = 0)]
    [string]$CustomVersion = "",

    [Parameter(Position = 1)]
    [Alias("Image", "App", "Project", "Mode")]
    [ValidateSet("all", "both", "goldex", "goldex-karat", "karat")]
    [string]$Target = "all"
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

if ($CustomVersion -in @("all", "both", "goldex", "goldex-karat", "karat")) {
    $Target = $CustomVersion
    $CustomVersion = ""
}

$Registry = "reg.goldexsoft.ir"
$Python = Get-Command python, python3 -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $Python) {
    throw "Python 3 is required to generate Docker build versions."
}

$Images = @()
if ($Target -in @("all", "both", "goldex")) {
    $Images += @{ Name = "goldex"; Dockerfile = "src/App/Server/GoldEx.Server/Dockerfile" }
}
if ($Target -in @("all", "both", "goldex-karat", "karat")) {
    $Images += @{ Name = "goldex-karat"; Dockerfile = "src/Calculator/Server/GoldEx.Calculator.Server/Dockerfile" }
}

Write-Host "Logging in to $Registry..." -ForegroundColor Yellow
docker login $Registry
if ($LASTEXITCODE -ne 0) { throw "Docker login failed." }

$VersionArguments = @("scripts/docker-version.py")
foreach ($Image in $Images) {
    $VersionArguments += @("--image", "$Registry/$($Image.Name)")
}
$GenerateArguments = $VersionArguments
if (-not [string]::IsNullOrWhiteSpace($CustomVersion)) {
    $GenerateArguments += @("--version", $CustomVersion)
}
$Version = & $Python.Source @GenerateArguments
if ($LASTEXITCODE -ne 0) { throw "Docker build version allocation failed." }
$Version = "$Version".Trim()

$Revision = git rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw "Cannot determine the Git revision." }

Write-Host "GoldEx Docker publish: $Version (target: $Target)" -ForegroundColor Cyan

# Build and publish immutable version tags before updating any latest alias.
foreach ($Image in $Images) {
    $VersionedImage = "$Registry/$($Image.Name):$Version"
    Write-Host "Building $VersionedImage..." -ForegroundColor Yellow
    docker build --tag $VersionedImage --build-arg "APP_VERSION=$Version" --build-arg "APP_REVISION=$Revision" --file $Image.Dockerfile .
    if ($LASTEXITCODE -ne 0) { throw "$($Image.Name) build failed." }
}
foreach ($Image in $Images) {
    docker push "$Registry/$($Image.Name):$Version"
    if ($LASTEXITCODE -ne 0) { throw "$($Image.Name) version push failed." }
}

# Re-check after the potentially long builds: a later CI/local build may have completed.
& $Python.Source @VersionArguments --check $Version
if ($LASTEXITCODE -ne 0) { throw "A newer build is already published; latest was not updated." }
foreach ($Image in $Images) {
    $LatestImage = "$Registry/$($Image.Name):latest"
    docker tag "$Registry/$($Image.Name):$Version" $LatestImage
    if ($LASTEXITCODE -ne 0) { throw "$($Image.Name) latest tag failed." }
    docker push $LatestImage
    if ($LASTEXITCODE -ne 0) { throw "$($Image.Name) latest push failed." }
}

Write-Host "Build & push completed. Published version: $Version" -ForegroundColor Green
Write-Host "Run this on the server to deploy:" -ForegroundColor Yellow
Write-Host "/home/user/docker/goldex/refresh-apps.sh $Version" -ForegroundColor Magenta
