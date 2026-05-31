Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Running Lab 2 REST API & Security Test Suite" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Wait for services to be ready
Write-Host "Waiting 5 seconds for web api container to be fully active..."
Start-Sleep -Seconds 5

$baseUrl = "http://localhost:8080"
$headers = @{
    "Content-Type" = "application/json"
}

# 2. Login to get JWT token
Write-Host "Test 1: POST /api/auth/login" -ForegroundColor Yellow
$loginBody = @{
    username = "admin"
    password = "123456"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method Post -Headers $headers -Body $loginBody -UseBasicParsing
    $loginJson = $loginResponse.Content | ConvertFrom-Json
    $token = $loginJson.data.accessToken
    Write-Host "Status Code: $($loginResponse.StatusCode) (OK)" -ForegroundColor Green
    Write-Host "Response Success: $($loginJson.success)" -ForegroundColor Green
    Write-Host "Access Token obtained successfully!" -ForegroundColor Green
} catch {
    Write-Host "FAILED: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errResp = $reader.ReadToEnd()
        Write-Host "Error details: $errResp" -ForegroundColor Red
    }
    Exit 1
}

# 3. Create Authorized Headers for subsequent GETs
$authHeaders = @{
    "Authorization" = "Bearer $token"
    "Accept" = "application/json"
}

$endpoints = @(
    @{ name = "GET /api/v1/students?page=1&size=5"; path = "/api/v1/students?page=1&size=5" },
    @{ name = "GET /api/v2/students?page=1&size=5"; path = "/api/v2/students?page=1&size=5" },
    @{ name = "GET /api/v1/courses?page=1&size=5";  path = "/api/v1/courses?page=1&size=5" },
    @{ name = "GET /api/v1/subjects?page=1&size=5";  path = "/api/v1/subjects?page=1&size=5" },
    @{ name = "GET /api/v1/semesters?page=1&size=5"; path = "/api/v1/semesters?page=1&size=5" },
    @{ name = "GET /api/v1/enrollments?page=1&size=5"; path = "/api/v1/enrollments?page=1&size=5" }
)

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "Testing Protected Endpoints with JWT Token" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

foreach ($ep in $endpoints) {
    Write-Host "Testing endpoint: $($ep.name)" -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl$($ep.path)" -Method Get -Headers $authHeaders -UseBasicParsing
        $json = $response.Content | ConvertFrom-Json
        Write-Host "Status Code: $($response.StatusCode) (OK)" -ForegroundColor Green
        Write-Host "Response Success: $($json.success)" -ForegroundColor Green
        Write-Host "Sample Data Count: $($json.data.Count)" -ForegroundColor Green
    } catch {
        Write-Host "FAILED: $_" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errResp = $reader.ReadToEnd()
            Write-Host "Error details: $errResp" -ForegroundColor Red
        }
    }
    Write-Host "------------------------------------------"
}
