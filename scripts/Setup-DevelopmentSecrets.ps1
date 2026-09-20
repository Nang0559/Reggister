$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "..\FVN_REGISTER.API\FVN_REGISTER.API.csproj"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK was not found in PATH."
}

$randomBytes = New-Object byte[] 48
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()

try {
    $rng.GetBytes($randomBytes)
}
finally {
    $rng.Dispose()
}

$jwtSecret = [Convert]::ToBase64String($randomBytes)

dotnet user-secrets set "Jwt:SecretKey" $jwtSecret --project $project
dotnet user-secrets set "Jwt:Issuer" "FVNRGTApi" --project $project
dotnet user-secrets set "Jwt:Audience" "FVNRGTUI" --project $project

$secureConnectionString = Read-Host "Enter SQL Server connection string" -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureConnectionString)

try {
    $connectionString = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        throw "Connection string cannot be empty."
    }

    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString --project $project
}
finally {
    if ($ptr -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

Write-Host ""
Write-Host "Development secrets configured successfully."
Write-Host "Verify with:"
Write-Host "  dotnet user-secrets list --project $project"
