[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Title,

    [string]$OutputDir = "docs/change-records",
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$workspaceRoot = (Get-Location).Path
$templatePath = Join-Path $workspaceRoot "templates/reporting/change-record.template.md"

if (-not (Test-Path $templatePath)) {
    throw "Template not found: $templatePath"
}

$date = Get-Date -Format "yyyy-MM-dd"
$safeTitle = ($Title.Trim() -replace '[^A-Za-z0-9\- ]', '').Replace(' ', '-').ToLowerInvariant()
$fileName = "$date-$safeTitle-change-record.md"
$outputRoot = Join-Path $workspaceRoot $OutputDir
$outputPath = Join-Path $outputRoot $fileName

if ((Test-Path $outputPath) -and -not $Force) {
    throw "File already exists: $outputPath. Use -Force to overwrite."
}

New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null

$content = Get-Content -Raw $templatePath
$content = $content -replace "- Date:\s*", "- Date: $date"
$content = $content -replace "- Title:\s*", "- Title: $Title"

Set-Content -Path $outputPath -Value $content -NoNewline
Write-Host "Created: $outputPath"