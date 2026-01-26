@echo off
echo ===================================
echo Step Up - First Time Setup
echo ===================================
echo.
echo Installing dependencies...
echo.

cd Frontend

echo Checking if node_modules exists...
if not exist "node_modules\" (
    echo Installing npm packages...
    call npm install
) else (
    echo Dependencies already installed.
)

echo.
echo ===================================
echo Starting development server...
echo ===================================
echo.
echo The login page will be available at:
echo http://localhost:5173/login
echo.
echo Press Ctrl+C to stop the server
echo.

call npm run dev

pause
