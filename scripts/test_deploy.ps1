# Exercise publishing orchestration with command mocks; never contact Docker or Git.
$ErrorActionPreference = "Stop"
$DeployScript = Join-Path $PSScriptRoot "../deploy.ps1"

function Get-Command {
    [pscustomobject]@{ Source = $global:GoldExTestMockPython }
}

$global:GoldExTestMockPython = {
    $global:LASTEXITCODE = 0
    if ($args -contains "--check") {
        if ($global:GoldExTestRejectPromotion) { $global:LASTEXITCODE = 1 }
    } else {
        Write-Output "1.2.125.42"
    }
}

function docker {
    $global:GoldExTestDockerCalls.Add(($args -join " "))
    $global:LASTEXITCODE = 0
    if ($global:GoldExTestFailBuild -and $args[0] -eq "build") { $global:LASTEXITCODE = 1 }
    if ($global:GoldExTestFailVersionPush -and $args[0] -eq "push" -and $args[1] -notlike "*:latest") { $global:LASTEXITCODE = 1 }
}

function git {
    $global:LASTEXITCODE = 0
    Write-Output "mock-commit"
}

function Assert-True($Condition, $Message) {
    if (-not $Condition) { throw $Message }
}

foreach ($Case in @("all", "goldex", "karat", "failed-build", "failed-push", "stale-promotion")) {
    $global:GoldExTestDockerCalls = [System.Collections.Generic.List[string]]::new()
    $global:GoldExTestFailBuild = $Case -eq "failed-build"
    $global:GoldExTestFailVersionPush = $Case -eq "failed-push"
    $global:GoldExTestRejectPromotion = $Case -eq "stale-promotion"
    $Target = if ($Case -in @("goldex", "karat")) { $Case } else { "all" }
    $Failed = $false
    $FailureMessage = ""
    try { & $DeployScript $Target *> $null } catch { $Failed = $true; $FailureMessage = $_.Exception.Message }
    $Calls = $global:GoldExTestDockerCalls.ToArray()
    $Promotions = @($Calls | Where-Object { $_ -like "tag *" -or $_ -like "push *:latest" })
    if ($Case -in @("failed-build", "failed-push", "stale-promotion")) {
        Assert-True $Failed "$Case should stop publishing."
        Assert-True ($Promotions.Count -eq 0) "$Case must not promote latest."
    } else {
        Assert-True (-not $Failed) "$Case should succeed: $FailureMessage"
        $ExpectedImages = if ($Case -eq "all") { 2 } else { 1 }
        Assert-True ($Promotions.Count -eq 2 * $ExpectedImages) "$Case must promote selected images only."
        $FirstTag = [Array]::FindIndex($Calls, [Predicate[string]]{ param($Call) $Call -like "tag *" })
        $VersionPushesBeforePromotion = @($Calls[0..($FirstTag - 1)] | Where-Object { $_ -like "push *:1.2.125.42" })
        Assert-True ($VersionPushesBeforePromotion.Count -eq $ExpectedImages) "Versioned images must be pushed before latest."
        if ($Case -eq "goldex") {
            Assert-True (-not ($Calls -match "goldex-karat")) "GoldEx-only publishing must not touch Karat."
        }
        if ($Case -eq "karat") {
            Assert-True (-not ($Calls -match "goldex:")) "Karat-only publishing must not touch GoldEx."
        }
    }
    Write-Output "PASS: $Case"
}
