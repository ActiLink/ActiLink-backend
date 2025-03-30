@echo off
setlocal enabledelayedexpansion

set "ENV_FILE=.env"
set "VAR_NAME=JWT_SECRET"

:: Generate 256 bit key
for /f "delims=" %%A in ('powershell -Command "[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))"') do (
    set "VAR_VALUE=%%A"
)

:: Create file if necessary
if not exist "%ENV_FILE%" type nul > "%ENV_FILE%"

set "FOUND=false"

:: Write other variables
(
    for /f "usebackq delims=" %%L in ("%ENV_FILE%") do (
        set "line=%%L"
        set "trimmed=%%L"

        rem 
        echo !trimmed! | findstr /B /C:"%VAR_NAME%=" >nul
        if !errorlevel! == 0 (
            echo %VAR_NAME%=%VAR_VALUE%
            set "FOUND=true"
        ) else (
            echo !line!
        )
    )

    rem 
    if "!FOUND!"=="false" (
        echo %VAR_NAME%=%VAR_VALUE%
    )
) > "%ENV_FILE%.tmp"

move /Y "%ENV_FILE%.tmp" "%ENV_FILE%" >nul

echo Variable %VAR_NAME% created and saved in %ENV_FILE%
endlocal