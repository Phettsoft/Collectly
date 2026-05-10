@echo off
echo ============================================
echo  Collectly - Clean Build Artifacts
echo  Kills locking processes and removes obj/bin
echo ============================================
echo.
echo Make sure Visual Studio is CLOSED before running.
echo.
pause

echo.
echo [1/6] Killing processes that lock build files...
taskkill /f /im "devenv.exe" 2>nul
taskkill /f /im "MSBuild.exe" 2>nul
taskkill /f /im "dotnet.exe" 2>nul
taskkill /f /im "java.exe" 2>nul
taskkill /f /im "adb.exe" 2>nul
taskkill /f /im "aapt2.exe" 2>nul
taskkill /f /im "VBCSCompiler.exe" 2>nul
taskkill /f /im "ServiceHub.Host.dotnet.x64.exe" 2>nul
taskkill /f /im "ServiceHub.RoslynCodeAnalysisService.exe" 2>nul

echo Waiting for processes to fully terminate...
timeout /t 4 /nobreak >nul

echo [2/6] Removing Collectly\obj and bin...
rmdir /s /q "Collectly\obj" 2>nul
rmdir /s /q "Collectly\bin" 2>nul

echo [3/6] Removing Collectly.Tests\obj and bin...
rmdir /s /q "Collectly.Tests\obj" 2>nul
rmdir /s /q "Collectly.Tests\bin" 2>nul

echo [4/6] Removing .vs cache...
rmdir /s /q ".vs" 2>nul

echo [5/6] Restoring NuGet packages...
dotnet restore Collectly.slnx

echo [6/6] Setting Collectly as startup project...
if not exist ".vs" mkdir ".vs"
if not exist ".vs\Collectly" mkdir ".vs\Collectly"
(
echo {
echo   "startup_project": "Collectly\\Collectly.csproj"
echo }
) > ".vs\Collectly\startup.json"

echo.
echo ============================================
echo  DONE! All build artifacts cleaned.
echo.
echo  Next step:
echo    Open Collectly.slnx in Visual Studio
echo    Right-click Collectly project ^> Set as Startup Project
echo    Build and deploy to Android
echo ============================================
echo.
pause
