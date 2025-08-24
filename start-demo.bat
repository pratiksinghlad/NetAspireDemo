@echo off
echo =================================
echo NetAspireDemo - Quick Start Demo
echo =================================
echo.

echo Checking .NET version...
dotnet --version
echo.

echo Available options:
echo.
echo 1. Individual Services (RECOMMENDED for now)
echo 2. Docker Compose
echo 3. Try Aspire AppHost (may fail with .NET 10 preview)
echo.

set /p choice="Enter your choice (1-3): "

if "%choice%"=="1" goto individual
if "%choice%"=="2" goto docker
if "%choice%"=="3" goto aspire
goto invalid

:individual
echo.
echo Starting individual services...
echo Press Ctrl+C in each window to stop
echo.
echo Starting Weather API in new window...
start "Weather API" cmd /k "cd /d src\WeatherApi && dotnet run"
timeout /t 3 /nobreak > nul

echo Starting Weather Web in new window...
start "Weather Web" cmd /k "cd /d src\WeatherWeb && dotnet run"

echo.
echo Services starting! Check the opened windows.
echo Weather API: http://localhost:5001/swagger
echo Weather Web: http://localhost:5000
goto end

:docker
echo.
echo Starting with Docker Compose...
docker compose up --build
goto end

:aspire
echo.
echo Attempting to start Aspire AppHost...
echo (This may fail with .NET 10 preview - use option 1 instead)
cd src\NetAspireDemo.AppHost
dotnet run
goto end

:invalid
echo Invalid choice! Please run the script again and choose 1, 2, or 3.
goto end

:end
echo.
pause
