@echo off
echo ============================================
echo  Collectly Project Rename Script
echo  Removes '#' from all folder/file names
echo ============================================
echo.
echo IMPORTANT: Make sure Visual Studio is CLOSED before running this.
echo.
pause

cd /d "c:\Users\gphet\source\repos"

echo.
echo [1/6] Renaming project file...
rename "# Collectly\# Collectly\# Collectly.csproj" "Collectly.csproj"
if %errorlevel% neq 0 (echo FAILED - is VS still open? & pause & exit /b 1)

echo [2/6] Renaming .csproj.user file...
if exist "# Collectly\# Collectly\# Collectly.csproj.user" (
    rename "# Collectly\# Collectly\# Collectly.csproj.user" "Collectly.csproj.user"
)

echo [3/6] Renaming project folder...
rename "# Collectly\# Collectly" "Collectly"
if %errorlevel% neq 0 (echo FAILED to rename project folder & pause & exit /b 1)

echo [4/6] Renaming solution file...
rename "# Collectly\# Collectly.slnx" "Collectly.slnx"
if %errorlevel% neq 0 (echo FAILED to rename solution file & pause & exit /b 1)

echo [5/6] Renaming solution folder...
rename "# Collectly" "Collectly"
if %errorlevel% neq 0 (echo FAILED to rename solution folder & pause & exit /b 1)

echo [6/6] Updating file references...

:: Update solution file to point to new project path
cd /d "c:\Users\gphet\source\repos\Collectly"
powershell -Command "(Get-Content 'Collectly.slnx') -replace '# Collectly/# Collectly.csproj', 'Collectly/Collectly.csproj' | Set-Content 'Collectly.slnx'"
powershell -Command "(Get-Content 'Collectly.slnx') -replace '# Collectly\\# Collectly.csproj', 'Collectly\\Collectly.csproj' | Set-Content 'Collectly.slnx'"

:: Update test project reference
powershell -Command "(Get-Content 'Collectly.Tests\Collectly.Tests.csproj') -replace '\\# Collectly\\# Collectly.csproj', '\Collectly\Collectly.csproj' | Set-Content 'Collectly.Tests\Collectly.Tests.csproj'"
powershell -Command "(Get-Content 'Collectly.Tests\Collectly.Tests.csproj') -replace '# Collectly\\# Collectly.csproj', 'Collectly\\Collectly.csproj' | Set-Content 'Collectly.Tests\Collectly.Tests.csproj'"

:: Remove the backup .tmp file if it exists
if exist "Collectly\Collectly.csproj.Backup.tmp" del "Collectly\Collectly.csproj.Backup.tmp"

echo.
echo ============================================
echo  DONE! 
echo.
echo  New structure:
echo    c:\Users\gphet\source\repos\Collectly\
echo      Collectly\Collectly.csproj
echo      Collectly.Tests\Collectly.Tests.csproj
echo      Collectly.slnx
echo.
echo  Open: c:\Users\gphet\source\repos\Collectly\Collectly.slnx
echo ============================================
echo.
pause
