@echo off
cd /d "%~dp0"
if exist "Assets\MMD4Mecanim\Editor" (
    ren "Assets\MMD4Mecanim\Editor" "Editor~"
    echo [SUCCESS] MMD4Mecanim Editor folder has been disabled.
    echo Unity errors should stop now.
) else (
    echo [INFO] Folder already renamed or not found.
)
pause
