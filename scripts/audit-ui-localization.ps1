[CmdletBinding()]
param(
    [string]$Root = (Join-Path $PSScriptRoot '..\FVN_REGISTER\FVN_REGISTER.Shared')
)

$ErrorActionPreference = 'Stop'
$extensions = '*.razor','*.cs'
$excluded = '\\bin\\|\\obj\\|\\wwwroot\\|\\Services\\Language\\'
$findings = [System.Collections.Generic.List[object]]::new()

$literalPatterns = @(
    '<MudButton[^>]*>\s*([^<@][^<]*)<',
    '<MudText[^>]*>\s*([^<@][^<]*)<',
    '<MudAlert[^>]*>\s*([^<@][^<]*)<',
    '<MudTooltip[^>]*Text="([^"]+)"',
    'Text="([A-Za-zÀ-ỹ][^"]*)"',
    'Label="([A-Za-zÀ-ỹ][^"]*)"',
    'Placeholder="([A-Za-zÀ-ỹ][^"]*)"',
    'Title="([A-Za-zÀ-ỹ][^"]*)"'
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
                    if ($value -and $value -notmatch '^(@|\{|\}|Icons\.|Color\.|Variant\.|Size\.|Typo\.)') {
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
    }

if ($findings.Count -eq 0) {
    Write-Host 'UI localization audit: no candidate literals found.' -ForegroundColor Green
    exit 0
}

Write-Host "UI localization audit: $($findings.Count) candidate(s) require review." -ForegroundColor Yellow
$findings | Sort-Object File,Line | Format-Table -AutoSize

# This is an audit, not an auto-rewriter. Business data/code must never be translated automatically.
exit 2
