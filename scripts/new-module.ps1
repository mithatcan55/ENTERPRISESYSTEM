[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ModuleName,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$TCodePrefix,

    [string]$ModuleRoute,
    [string]$OutputRoot = "scaffolds/generated",
    [switch]$Force,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$workspaceRoot = (Get-Location).Path
$templateRoot = Join-Path $workspaceRoot "templates/module-scaffold"

if (-not (Test-Path $templateRoot)) {
    throw "Template klasörü bulunamadı: $templateRoot"
}

if ([string]::IsNullOrWhiteSpace($ModuleRoute)) {
    $ModuleRoute = $ModuleName.ToLowerInvariant()
}

$prefixRaw = [regex]::Replace($TCodePrefix.Trim(), '[^A-Za-z]', '')
if ($prefixRaw.Length -lt 2 -or $prefixRaw.Length -gt 6) {
    throw "TCodePrefix 2 ile 6 karakter arasında olmalıdır."
}

$prefixUpper = $prefixRaw.ToUpperInvariant()
$moduleCode = $prefixUpper
$subModuleCode = "$prefixUpper-CORE"
$pageCode = "$prefixUpper-PAGE"

$targetRoot = Join-Path $workspaceRoot (Join-Path $OutputRoot $ModuleName)

if ((Test-Path $targetRoot) -and -not $Force) {
    throw "Hedef klasör zaten var: $targetRoot. Üzerine yazmak için -Force kullanın."
}

if ($DryRun) {
    Write-Host "[DRY-RUN] Scaffold üretilecek klasör: $targetRoot"
} else {
    New-Item -ItemType Directory -Path $targetRoot -Force | Out-Null
}

$files = Get-ChildItem -Path $templateRoot -Recurse -File -Filter "*.template"

foreach ($file in $files) {
    $relativePath = $file.FullName.Substring($templateRoot.Length)
    $relativePath = $relativePath -replace '^[\\/]+', ''
    $relativePath = $relativePath -replace '\\', '/'

    $targetRelative = $relativePath.Replace("module-template", $ModuleName)
    $targetRelative = $targetRelative.Replace("ModuleName", $ModuleName)
    $targetRelative = $targetRelative.Replace("module", $ModuleRoute)
    $targetRelative = $targetRelative.Replace(".template", "")

    $targetPath = Join-Path $targetRoot $targetRelative
    $targetDir = Split-Path $targetPath -Parent

    $content = Get-Content -Path $file.FullName -Raw
    $content = $content.Replace("{{ModuleName}}", $ModuleName)
    $content = $content.Replace("{{module-route}}", $ModuleRoute)
    $content = $content.Replace("{{TCODE_PREFIX}}", $prefixUpper)
    $content = $content.Replace("{{MODULE_CODE}}", $moduleCode)
    $content = $content.Replace("{{SUBMODULE_CODE}}", $subModuleCode)
    $content = $content.Replace("{{PAGE_CODE}}", $pageCode)

    if ($DryRun) {
        Write-Host "[DRY-RUN] $targetRelative"
        continue
    }

    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    Set-Content -Path $targetPath -Value $content -NoNewline
}

if ($DryRun) {
    Write-Host "[DRY-RUN] Tamamlandı. Dosya üretilmedi."
} else {
    Write-Host "Scaffold üretildi: $targetRoot"
}
