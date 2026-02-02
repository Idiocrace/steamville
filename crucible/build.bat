@echo off
REM Build and package Crucible as a library + publish runtime build
setlocal

set CONFIG=Beta
set PROJECT=Crucible.csproj
set OUT_NUGET=publish\nuget
set OUT_PUB=publish\win-x64
set RUNTIME=win-x64
set PACKAGE_VERSION=0.1

echo Building Solution
dotnet restore "%PROJECT%" || exit /b %ERRORLEVEL%

echo Building...
dotnet build "%PROJECT%" -c %CONFIG% || exit /b %ERRORLEVEL%

echo Packing NuGet package...
dotnet pack "%PROJECT%" -c %CONFIG% -o "%OUT_NUGET%" /p:PackageVersion=%PACKAGE_VERSION% || exit /b %ERRORLEVEL%

echo Publishing runtime build (for testing)...
dotnet publish "%PROJECT%" -c %CONFIG% -r %RUNTIME% --self-contained false -o "%OUT_PUB%" || exit /b %ERRORLEVEL%

echo Copying native runtimes (if present)...
if exist "bin\%CONFIG%\net9.0\runtimes\%RUNTIME%\native\" (
  xcopy /Y /E "bin\%CONFIG%\net9.0\runtimes\%RUNTIME%\native" "%OUT_PUB%\runtimes\%RUNTIME%\native\"
) else (
  echo No native runtimes found in bin output.
)

echo Done. NuGet at "%OUT_NUGET%" and runtime publish at "%OUT_PUB%".
endlocal