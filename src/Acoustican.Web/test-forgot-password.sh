#!/bin/bash
# Forgot Password Feature Test Script

API_URL="http://localhost:5000/api"
BASE_URL="http://localhost:5000"

echo "=== Forgot Password Feature Tests ==="
echo ""

# Test 1: Verify forgot password link on login page
echo "[Test 1] Checking if forgot password link exists on login page..."
LOGIN_PAGE=$(curl -s "$BASE_URL/admin")
if echo "$LOGIN_PAGE" | grep -q "Forgot password"; then
    echo "✓ PASS: Forgot password link found on admin login page"
else
    echo "✗ FAIL: Forgot password link not found on admin login page"
fi
echo ""

# Test 2: Verify forgot password page is accessible
echo "[Test 2] Testing forgot password page accessibility..."
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/forgot-password")
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
if [ "$HTTP_CODE" -eq 200 ]; then
    echo "✓ PASS: Forgot password page is accessible (HTTP $HTTP_CODE)"
else
    echo "✗ FAIL: Forgot password page returned HTTP $HTTP_CODE"
fi
echo ""

# Test 3: Test forgot password API with valid email
echo "[Test 3] Testing forgot password API with valid email..."
API_RESPONSE=$(curl -s -X POST "$API_URL/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@secumetrix.com"}')

if echo "$API_RESPONSE" | grep -q '"success":true'; then
    echo "✓ PASS: Forgot password API returned success"
    RESET_TOKEN=$(echo "$API_RESPONSE" | grep -o '"resetToken":"[^"]*' | cut -d'"' -f4)
    echo "  Reset Token generated: ${RESET_TOKEN:0:20}..."
else
    echo "✗ FAIL: Forgot password API returned error"
    echo "  Response: $API_RESPONSE"
fi
echo ""

# Test 4: Test forgot password API with invalid email
echo "[Test 4] Testing forgot password API with non-existent email..."
API_RESPONSE=$(curl -s -X POST "$API_URL/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{"email":"nonexistent@example.com"}')

if echo "$API_RESPONSE" | grep -q '"success":false'; then
    echo "✓ PASS: Forgot password API correctly rejected invalid email"
else
    echo "✗ FAIL: Forgot password API should reject non-existent email"
fi
echo ""

# Test 5: Test reset password page
echo "[Test 5] Testing reset password page with token..."
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/reset-password?token=test-token-123")
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
PAGE_CONTENT=$(echo "$RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ] && echo "$PAGE_CONTENT" | grep -q "Reset Password"; then
    echo "✓ PASS: Reset password page is accessible (HTTP $HTTP_CODE)"
    if echo "$PAGE_CONTENT" | grep -q "test-token-123"; then
        echo "✓ PASS: Token parameter is correctly passed to reset password page"
    else
        echo "✗ FAIL: Token not found in reset password page"
    fi
else
    echo "✗ FAIL: Reset password page returned HTTP $HTTP_CODE"
fi
echo ""

# Test 6: Verify password reset fails with invalid token
echo "[Test 6] Testing reset password API with invalid token..."
API_RESPONSE=$(curl -s -X POST "$API_URL/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d '{"token":"invalid-token","newPassword":"NewPassword123!"}')

if echo "$API_RESPONSE" | grep -q '"success":false'; then
    echo "✓ PASS: Reset password API correctly rejected invalid token"
else
    echo "✗ FAIL: Reset password API should reject invalid token"
fi
echo ""

# Test 7: Test login modal in home page
echo "[Test 7] Checking login modal in home page..."
HOME_PAGE=$(curl -s "$BASE_URL")
if echo "$HOME_PAGE" | grep -q "authModal\|Welcome Back"; then
    echo "✓ PASS: Login modal found in home page"
    if echo "$HOME_PAGE" | grep -q "Forgot password"; then
        echo "✓ PASS: Forgot password link found in home page login modal"
    else
        echo "✗ FAIL: Forgot password link not found in home page login modal"
    fi
else
    echo "✗ FAIL: Login modal not found in home page"
fi
echo ""

echo "=== Test Suite Complete ==="
