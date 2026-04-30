param(
    [string]$ApiBaseUrl = "http://localhost:8080",
    [string]$Email = "admin@example.com",
    [SecureString]$Password,
    [string]$Name = "Admin User",
    [string]$UserName = "admin",
    [int]$MediaExerciseLimit = 250,
    [int]$SyncTimeoutSeconds = 600,
    [switch]$StartDocker
)

$ErrorActionPreference = "Stop"

function Get-RepoRoot {
    return Split-Path -Parent $PSScriptRoot
}

function ConvertTo-PlainText {
    param([SecureString]$Value)

    if ($null -eq $Value) {
        return $null
    }

    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

function Read-DotEnvValue {
    param(
        [string]$Path,
        [string]$Key
    )

    if (-not (Test-Path $Path)) {
        return $null
    }

    $line = Get-Content $Path | Where-Object { $_ -match "^${Key}=" } | Select-Object -First 1
    if (-not $line) {
        return $null
    }

    return ($line -split "=", 2)[1]
}

function Read-UserSecretsConnectionString {
    param([string]$ProjectPath)

    try {
        $secrets = dotnet user-secrets list --project $ProjectPath 2>$null
        $line = $secrets | Where-Object { $_ -match '^ConnectionStrings:DefaultConnection\s*=\s*' } | Select-Object -First 1
        if (-not $line) {
            return $null
        }

        return ($line -split '=', 2)[1].Trim()
    }
    catch {
        return $null
    }
}

function Set-UserAdminRole {
    param(
        [string]$RepoRoot,
        [string]$Email,
        [SecureString]$DbSaPassword
    )

    $dockerSqlContainer = docker ps --filter "name=exercise_sqlserver" --format "{{.Names}}" | Select-Object -First 1
    if ($dockerSqlContainer -and $DbSaPassword) {
        $plainTextDbSaPassword = ConvertTo-PlainText -Value $DbSaPassword
        docker exec exercise_sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P $plainTextDbSaPassword -d ExerciseDb -Q "UPDATE Users SET Role = 'Admin' WHERE Email = '$Email'; SELECT Email, Role FROM Users WHERE Email = '$Email';"
        return
    }

    $projectPath = Join-Path $RepoRoot "Exercise.API\Exercise.API.csproj"
    $connectionString = Read-UserSecretsConnectionString -ProjectPath $projectPath
    if (-not $connectionString) {
        throw "No Docker SQL container is running and no ConnectionStrings:DefaultConnection value was found in user-secrets."
    }

    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "UPDATE Users SET Role = 'Admin' WHERE Email = @email; SELECT Role FROM Users WHERE Email = @email;"
        [void]$command.Parameters.Add("@email", [System.Data.SqlDbType]::NVarChar, 256)
        $command.Parameters["@email"].Value = $Email
        $result = $command.ExecuteScalar()
        if ($result -ne 'Admin') {
            throw "Failed to promote '$Email' to Admin using the configured SQL connection."
        }
    }
    finally {
        $connection.Dispose()
    }
}

function Wait-ApiReady {
    param([string]$BaseUrl)

    Write-Host "Waiting for API health at $BaseUrl/health..."
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try {
            Invoke-RestMethod -Method Get -Uri "$BaseUrl/health" -TimeoutSec 5 | Out-Null
            Write-Host "API is healthy."
            return
        }
        catch {
            Start-Sleep -Seconds 2
        }
    }

    throw "API did not become ready at $BaseUrl"
}

function Invoke-JsonRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body,
        [hashtable]$Headers,
        [int]$TimeoutSeconds = 30
    )

    $params = @{
        Method = $Method
        Uri = $Uri
        Headers = $Headers
    }

    if ($null -ne $Body) {
        $params.ContentType = "application/json"
        $params.Body = ($Body | ConvertTo-Json -Depth 6)
    }

    $params.TimeoutSec = $TimeoutSeconds

    return Invoke-RestMethod @params
}

$repoRoot = Get-RepoRoot
$envPath = Join-Path $repoRoot ".env"
$dbSaPassword = Read-DotEnvValue -Path $envPath -Key "DB_SA_PASSWORD"

if ($null -eq $Password) {
    $Password = ConvertTo-SecureString "StrongPass!123" -AsPlainText -Force
}

$plainTextPassword = ConvertTo-PlainText -Value $Password
$dbSaSecurePassword = if ($dbSaPassword) { ConvertTo-SecureString $dbSaPassword -AsPlainText -Force } else { $null }

if ($StartDocker) {
    Write-Host "Starting Docker Compose services..."
    Push-Location $repoRoot
    try {
        docker compose up -d
    }
    finally {
        Pop-Location
    }
}

Wait-ApiReady -BaseUrl $ApiBaseUrl

try {
    Write-Host "Attempting login for $Email..."
    $loginResponse = Invoke-JsonRequest -Method Post -Uri "$ApiBaseUrl/api/auth/login" -Body @{
        email = $Email
        password = $plainTextPassword
    } -Headers @{} -TimeoutSeconds 30
}
catch {
    Write-Host "Login failed. Registering operator account for $Email..."
    Invoke-JsonRequest -Method Post -Uri "$ApiBaseUrl/api/users/register" -Body @{
        name = $Name
        email = $Email
        password = $plainTextPassword
        userName = $UserName
    } -Headers @{} -TimeoutSeconds 30 | Out-Null

    Write-Host "Promoting $Email to Admin..."
    Set-UserAdminRole -RepoRoot $repoRoot -Email $Email -DbSaPassword $dbSaSecurePassword

    Write-Host "Retrying login after promotion..."
    $loginResponse = Invoke-JsonRequest -Method Post -Uri "$ApiBaseUrl/api/auth/login" -Body @{
        email = $Email
        password = $plainTextPassword
    } -Headers @{} -TimeoutSeconds 30
}

if ($loginResponse.role -ne "Admin") {
    Write-Host "User authenticated but is not Admin. Promoting role and retrying login..."
    Set-UserAdminRole -RepoRoot $repoRoot -Email $Email -DbSaPassword $dbSaSecurePassword

    $loginResponse = Invoke-JsonRequest -Method Post -Uri "$ApiBaseUrl/api/auth/login" -Body @{
        email = $Email
        password = $plainTextPassword
    } -Headers @{} -TimeoutSeconds 30
}

Write-Host "Running exercise sync with mediaExerciseLimit=$MediaExerciseLimit..."
$syncResponse = Invoke-JsonRequest -Method Post -Uri "$ApiBaseUrl/api/exercises/sync?mediaExerciseLimit=$MediaExerciseLimit" -Body $null -Headers @{
    Authorization = "Bearer $($loginResponse.token)"
} -TimeoutSeconds $SyncTimeoutSeconds

Write-Host "Sync completed."
$syncResponse | ConvertTo-Json -Depth 6