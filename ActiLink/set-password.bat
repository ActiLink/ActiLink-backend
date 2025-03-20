@echo off
setlocal enabledelayedexpansion

:: check if password argument was given
if "%~1"=="" exit /b 1

:: Ustawienie zmiennej
set VAR_NAME=MSSQL_SA_PASSWORD
set VAR_VALUE=%~1

:: check if .env exists
if not exist .env echo. > .env

:: check if the variable MSSQL_SA_PASSWORD is defined
findstr /R "^%VAR_NAME%=" .env >nul
if %errorlevel%==0 (
    (
        for /f "tokens=1,* delims==" %%A in (.env) do (
            if "%%A"=="%VAR_NAME%" (
                echo %VAR_NAME%=%VAR_VALUE%
            ) else (
                echo %%A=%%B
            )
        )
    ) > .env.tmp
    move /Y .env.tmp .env >nul
) else (
    echo %VAR_NAME%=%VAR_VALUE%>> .env
)

endlocal