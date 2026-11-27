$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot '..\src\Salary360.API'

function New-RandomToken([int]$Length) {
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $bytes = New-Object byte[] 64
        $rng.GetBytes($bytes)
        $token = [Convert]::ToBase64String($bytes) -replace '[^A-Za-z0-9]', ''
        if ($token.Length -lt $Length) {
            throw "Generated token was shorter than $Length characters."
        }
        return $token.Substring(0, $Length)
    }
    finally {
        $rng.Dispose()
    }
}

function Read-SecretPassword([string]$Label) {
    $secure = Read-Host "$Label (leave empty to generate)" -AsSecureString
    $plain = [System.Net.NetworkCredential]::new('', $secure).Password
    if ([string]::IsNullOrEmpty($plain)) {
        $plain = 'Aa1-' + (New-RandomToken 16)
        Write-Host "Generated password for ${Label}: $plain"
    }
    return $plain
}

dotnet user-secrets init --project $project | Out-Null

dotnet user-secrets set 'Jwt:Secret' (New-RandomToken 64) --project $project | Out-Null
dotnet user-secrets set 'Seed:Admin:Password' (Read-SecretPassword 'Seed Admin password') --project $project | Out-Null
dotnet user-secrets set 'Seed:User:Password' (Read-SecretPassword 'Seed User password') --project $project | Out-Null

Write-Host 'User secrets configured. Keys stored:'
dotnet user-secrets list --project $project | ForEach-Object { ($_ -split '=')[0] + '= ********' }
Write-Host 'Generated passwords are shown only once above; delete salary360.db to recreate the seed users.'
