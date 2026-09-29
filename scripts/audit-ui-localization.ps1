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
$literalPatterns = @(
    '<MudButton[^>]*>\s*([^<@][^<]*)<',
    '<MudText[^>]*>\s*([^<@][^<]*)<',
    '<MudAlert[^>]*>\s*([^<@][^<]*)<',
    '<MudTh[^>]*>\s*([^<@][^<]*)<',
    '<MudTd[^>]*>\s*([^<@][^<]*)<',
    '<MudTabPanel[^>]*Text="([^"]+)"',
    '<MudTooltip[^>]*Text="([^"]+)"',
    'Text="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Label="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Placeholder="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Title="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'aria-label="([A-Za-zÀ-ỹぁ-んァ-ヶ一-龯][^"]*)"',
    'Snackbar\.Add\(\s*"([^"]+)"',
    'Snackbar\.Add\(\s*\$"([^"]+)"'
)

$ignoreValues = @(
    'true','false','submit','button','text','password','email','GET','POST','PUT','DELETE',
    'Serial Number','Request','QR','OT','HRM','FCC','FVN REGISTER'
)

Get-ChildItem -Path $Root -Recurse -File -Include $extensions |
    Where-Object { $_.FullName -notmatch $excluded } |
    ForEach-Object {
        $file = $_
        $lineNo = 0
        Get-Content -LiteralPath $file.FullName | ForEach-Object {
            $lineNo++
            $line = $_
            foreach ($pattern in $literalPatterns) {
                if ($line -match $pattern) {
                    $value = $Matches[1].Trim()
                    if (-not $value -or $value -in $ignoreValues) { continue }
                    if ($value -match '^(@|\{|\}|Icons\.|Color\.|Variant\.|Size\.|Typo\.|Mud|http|/|api/)') { continue }
                    if ($value -match '^Language\.T\(') { continue }

                    $findings.Add([pscustomobject]@{
                        File = $file.FullName.Substring((Resolve-Path $Root).Path.Length).TrimStart('\\')
                        Line = $lineNo
                        Text = $value
                        Pattern = $pattern
                    })
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
