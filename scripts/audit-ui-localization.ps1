[CmdletBinding()]
param(
    [string]$Root = (Join-Path $PSScriptRoot '..\FVN_REGISTER\FVN_REGISTER.Shared'),
    [switch]$Strict
)

$ErrorActionPreference = 'Stop'
$extensions = '*.razor','*.cs'
$excluded = '\\bin\\|\\obj\\|\\wwwroot\\|\\Services\\Language\\'
$findings = [System.Collections.Generic.List[object]]::new()

# Presentation literals only. API/Core/Application/Infrastructure are intentionally out of scope.
# The audit is deliberately conservative: every candidate must be reviewed rather than silently ignored.
$literalPatterns = @(
    '<PageTitle>\s*([^<@][^<]*)<',
    '<MudButton[^>]*>\s*([^<@][^<]*)<',
    '<MudText[^>]*>\s*([^<@][^<]*)<',
    '<MudAlert[^>]*>\s*([^<@][^<]*)<',
    '<MudTh[^>]*>\s*([^<@][^<]*)<',
    '<MudTd[^>]*>\s*([^<@][^<]*)<',
    '<MudTabPanel[^>]*Text="([^"]+)"',
    '<MudTooltip[^>]*Text="([^"]+)"',
    '<MudSelect[^>]*Label="([^"]+)"',
    '<MudTextField[^>]*Label="([^"]+)"',
    '<MudTextField[^>]*Placeholder="([^"]+)"',
    '<MudNumericField[^>]*Label="([^"]+)"',
    '<MudDatePicker[^>]*Label="([^"]+)"',
    '<MudTimePicker[^>]*Label="([^"]+)"',
    '<MudCheckBox[^>]*Label="([^"]+)"',
    '<MudSwitch[^>]*Label="([^"]+)"',
    '<MudRadio[^>]*Label="([^"]+)"',
    'DataLabel="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Text="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Label="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Placeholder="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Title="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'aria-label="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'alt="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Snackbar\.Add\(\s*"([^"]+)"',
    'Snackbar\.Add\(\s*\$"([^"]+)"',
    'ShowAsync<[^>]+>\(\s*"([^"]+)"',
    'ShowAsync<[^>]+>\(\s*\$"([^"]+)"',
    'ShowMessageBoxAsync\(\s*"([^"]+)"',
    'ShowMessageBoxAsync\(\s*\$"([^"]+)"'
)

$ignoreValues = @(
    'true','false','submit','button','text','password','email','GET','POST','PUT','DELETE',
    'Serial Number','Request','QR','OT','HRM','FCC','FVN REGISTER'
)

function Add-Finding([string]$file, [int]$lineNo, [string]$value, [string]$pattern) {
    $value = $value.Trim()
    if (-not $value -or $value -in $ignoreValues) { return }
    if ($value -match '^(@|\{|\}|Icons\.|Color\.|Variant\.|Size\.|Typo\.|Mud|http|/|api/)') { return }
    if ($value -match '^Language\.T\(') { return }
    if ($value -match '^\d+(\.\d+)?$') { return }
    $findings.Add([pscustomobject]@{
        File = $file
        Line = $lineNo
        Text = $value
        Pattern = $pattern
    })
}

$rootPath = (Resolve-Path $Root).Path
Get-ChildItem -Path $Root -Recurse -File -Include $extensions |
    Where-Object { $_.FullName -notmatch $excluded } |
    ForEach-Object {
        $file = $_
        $relative = $file.FullName.Substring($rootPath.Length).TrimStart('\\')
        $lineNo = 0
        Get-Content -LiteralPath $file.FullName | ForEach-Object {
            $lineNo++
            $line = $_
            foreach ($pattern in $literalPatterns) {
                if ($line -match $pattern) {
                    Add-Finding $relative $lineNo $Matches[1] $pattern
                }
            }
        }
    }

$findings = $findings | Sort-Object File,Line,Text -Unique

if ($findings.Count -eq 0) {
    Write-Host 'UI localization audit: no candidate literals found.' -ForegroundColor Green
    exit 0
}

Write-Host "UI localization audit: $($findings.Count) candidate(s) require review." -ForegroundColor Yellow
$findings | Format-Table -AutoSize

if ($Strict) { exit 2 }
exit 1
