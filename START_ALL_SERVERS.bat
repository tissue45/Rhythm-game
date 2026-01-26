@echo off
echo ===================================
echo Starting All Servers
echo ===================================
echo.
echo 1. Unity Auth Bridge Server (port 3001)
echo 2. Frontend Dev Server (port 5173)
echo.
echo Both servers will start in separate windows
echo.

REM Unity Auth Server 시작
start "Unity Auth Server" cmd /k "echo Starting Unity Auth Server... && node unity_auth_server.js"

REM 2초 대기
timeout /t 2 /nobreak > nul

REM Frontend Dev Server 시작
start "Frontend Dev Server" cmd /k "cd Frontend && echo Starting Frontend Dev Server... && npm run dev"

echo.
echo ===================================
echo All servers started!
echo ===================================
echo.
echo Unity Auth Server: http://localhost:3001
echo Frontend: http://localhost:5173
echo.
echo Unity Login URL: http://localhost:5173/unity-login?unity=true
echo.
echo Close the server windows to stop them
echo.

pause
