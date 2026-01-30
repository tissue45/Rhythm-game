@echo off
echo ===================================
echo Unity Authentication Bridge Server
echo ===================================
echo.
echo This server bridges web login and Unity game
echo Server will run on port 3001
echo.

REM Check if node_modules exists
if not exist "node_modules\" (
    echo Installing dependencies...
    call npm install express cors
    echo.
)

echo Starting Unity Auth Server...
echo Press Ctrl+C to stop the server
echo.

node unity_auth_server.js

pause
