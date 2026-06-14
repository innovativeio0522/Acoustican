param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$ApiUrl = "http://localhost:5000/api"
)

$testsPassed = 0
$testsFailed = 0

function Test-Case {
    param(
        [string]$TestName,
        [scriptblock]$TestScript
    )
    
    Write-Host ""
    Write-Host "Testing: $TestName" -ForegroundColor Cyan
    try {
        & $TestScript
        $testsPassed++
        Write-Host "✓ PASS" -ForegroundColor Green
    } catch {
        $testsFailed++
        Write-Host "✗ FAIL: $_" -ForegroundColor Red
    }
}

Write-Host "=== Forgot Password Feature Test Suite ===" -ForegroundColor Yellow
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "API URL: $ApiUrl" -ForegroundColor Gray
Write-Host ""

# Test 1: Login page has forgot password link
Test-Case "Admin login page has forgot password link" {
    $response = Invoke-WebRequest -Uri "$BaseUrl/admin" -ErrorAction Stop
    if ($response.Content -match "Forgot password") {
        Write-Host "  ✓ Forgot password link found on admin login page"
    } else {
        throw "Forgot password link not found"
    }
}

# Test 2: Forgot password page is accessible
Test-Case "Forgot password page is accessible" {
    $response = Invoke-WebRequest -Uri "$BaseUrl/forgot-password" -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "  ✓ Page returned HTTP 200"
        if ($response.Content -match "Forgot Password") {
            Write-Host "  ✓ Page contains 'Forgot Password' title"
        } else {
            throw "Page doesn't contain expected title"
        }
    } else {
        throw "Unexpected status code: $($response.StatusCode)"
    }
}

# Test 3: Forgot password API with valid email
Test-Case "Forgot password API accepts valid email" {
    $body = @{
        email = "admin@secumetrix.com"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -Uri "$ApiUrl/auth/forgot-password" `
        -Method POST `
        -Headers @{"Content-Type" = "application/json"} `
        -Body $body `
        -ErrorAction Stop
    
    $data = $response.Content | ConvertFrom-Json
    if ($data.success) {
        Write-Host "  ✓ API returned success: true"
        if ($data.resetToken) {
            Write-Host "  ✓ Reset token generated: $($data.resetToken.Substring(0, 20))..."
        }
    } else {
        throw "API returned success: false"
    }
}

# Test 4: Forgot password API rejects invalid email
Test-Case "Forgot password API rejects non-existent email" {
    $body = @{
        email = "nonexistent@invalid.example.com"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-WebRequest -Uri "$ApiUrl/auth/forgot-password" `
            -Method POST `
            -Headers @{"Content-Type" = "application/json"} `
            -Body $body `
            -ErrorAction Stop
        
        $data = $response.Content | ConvertFrom-Json
        if (-not $data.success) {
            Write-Host "  ✓ API correctly rejected non-existent email"
        } else {
            throw "API should reject non-existent email"
        }
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) {
            Write-Host "  ✓ API returned 400 Bad Request as expected"
        } else {
            throw $_
        }
    }
}

# Test 5: Reset password page is accessible
Test-Case "Reset password page is accessible with token" {
    $response = Invoke-WebRequest -Uri "$BaseUrl/reset-password?token=test-token-abc123" -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "  ✓ Page returned HTTP 200"
        if ($response.Content -match "Reset Password") {
            Write-Host "  ✓ Page contains 'Reset Password' title"
        }
        if ($response.Content -match "test-token-abc123") {
            Write-Host "  ✓ Token parameter is embedded in page"
        }
    } else {
        throw "Unexpected status code: $($response.StatusCode)"
    }
}

# Test 6: Reset password API rejects invalid token
Test-Case "Reset password API rejects invalid token" {
    $body = @{
        token = "invalid-token-xyz"
        newPassword = "NewPassword123!"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-WebRequest -Uri "$ApiUrl/auth/reset-password" `
            -Method POST `
            -Headers @{"Content-Type" = "application/json"} `
            -Body $body `
            -ErrorAction Stop
        
        $data = $response.Content | ConvertFrom-Json
        if (-not $data.success) {
            Write-Host "  ✓ API correctly rejected invalid token"
        } else {
            throw "API should reject invalid token"
        }
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) {
            Write-Host "  ✓ API returned 400 Bad Request as expected"
        } else {
            throw $_
        }
    }
}

# Test 7: Home page has login modal with forgot password
Test-Case "Home page login modal has forgot password link" {
    $response = Invoke-WebRequest -Uri "$BaseUrl" -ErrorAction Stop
    if ($response.Content -match "authModal|Welcome Back") {
        Write-Host "  ✓ Login modal found in home page"
        if ($response.Content -match "Forgot password") {
            Write-Host "  ✓ Forgot password link found in modal"
        } else {
            throw "Forgot password link not in modal"
        }
    } else {
        throw "Login modal not found"
    }
}

# Test 8: Password reset functionality (end-to-end)
Test-Case "End-to-end password reset flow" {
    # First, request password reset
    $forgotBody = @{
        email = "admin@secumetrix.com"
    } | ConvertTo-Json
    
    $forgotResponse = Invoke-WebRequest -Uri "$ApiUrl/auth/forgot-password" `
        -Method POST `
        -Headers @{"Content-Type" = "application/json"} `
        -Body $forgotBody `
        -ErrorAction Stop
    
    $forgotData = $forgotResponse.Content | ConvertFrom-Json
    if ($forgotData.success -and $forgotData.resetToken) {
        Write-Host "  ✓ Generated reset token"
        
        # Now try to reset password with the token
        $resetBody = @{
            token = $forgotData.resetToken
            newPassword = "NewTestPassword123!"
        } | ConvertTo-Json
        
        $resetResponse = Invoke-WebRequest -Uri "$ApiUrl/auth/reset-password" `
            -Method POST `
            -Headers @{"Content-Type" = "application/json"} `
            -Body $resetBody `
            -ErrorAction Stop
        
        $resetData = $resetResponse.Content | ConvertFrom-Json
        if ($resetData.success) {
            Write-Host "  ✓ Password reset successful"
        } else {
            throw "Password reset failed: $($resetData.message)"
        }
    } else {
        throw "Failed to generate reset token"
    }
}

# Print summary
Write-Host ""
Write-Host "=== Test Summary ===" -ForegroundColor Yellow
Write-Host "Passed: $testsPassed" -ForegroundColor Green
Write-Host "Failed: $testsFailed" -ForegroundColor $(if ($testsFailed -gt 0) { 'Red' } else { 'Green' })
Write-Host ""

if ($testsFailed -eq 0) {
    Write-Host "All tests passed! ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "$testsFailed test(s) failed!" -ForegroundColor Red
    exit 1
}
