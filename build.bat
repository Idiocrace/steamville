@echo off
REM Build script for TileGame - Creates a self-contained Windows executable

echo Building TileGame for Windows (x64)...
echo.

REM Clean previous builds
if exist "publish" (
    echo Cleaning previous build...
    rmdir /s /q publish
)

REM Build self-contained executable
dotnet publish TileGame.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output publish ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true

if %errorlevel% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b %errorlevel%
)

echo.
echo Build successful!
echo Executable location: publish\TileGame.exe
echo.
pause
