@echo off
setlocal
cd /d "%~dp0"

echo === [0/4] check tools ===
where winget >nul 2>&1
if errorlevel 1 (
    echo winget not found: install dotnet SDK and VS C++ workload manually
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [setup] installing dotnet SDK 10 ...
    winget install --id Microsoft.DotNet.SDK.10 -e --accept-source-agreements --accept-package-agreements
    if errorlevel 1 (
        echo dotnet SDK install failed
        exit /b 1
    )
    set "PATH=%PATH%;C:\Program Files\dotnet"
)

set "VCVARS="
for %%V in ("18\Community" "18\Professional" "18\Enterprise" "17\Community" "17\Professional" "17\Enterprise") do (
    if not defined VCVARS if exist "C:\Program Files\Microsoft Visual Studio\%%~V\VC\Auxiliary\Build\vcvars64.bat" (
        set "VCVARS=C:\Program Files\Microsoft Visual Studio\%%~V\VC\Auxiliary\Build\vcvars64.bat"
    )
)
if not defined VCVARS (
    echo [setup] installing Visual Studio 2026 Community with C++ workload ...
    winget install --id Microsoft.VisualStudio.2026.Community -e --accept-source-agreements --accept-package-agreements --override "--quiet --add Microsoft.VisualStudio.Workload.NativeDesktop --includeRecommended"
    if errorlevel 1 (
        echo Visual Studio install failed
        exit /b 1
    )
    for %%V in ("18\Community" "18\Professional" "18\Enterprise" "17\Community" "17\Professional" "17\Enterprise") do (
        if not defined VCVARS if exist "C:\Program Files\Microsoft Visual Studio\%%~V\VC\Auxiliary\Build\vcvars64.bat" (
            set "VCVARS=C:\Program Files\Microsoft Visual Studio\%%~V\VC\Auxiliary\Build\vcvars64.bat"
        )
    )
)
if not defined VCVARS (
    echo vcvars64 still not found after install
    exit /b 1
)
echo tools ok: dotnet + vcvars64

echo === [1/4] build patcher (C#) ===
rem -p:ILRepack=false：跳过合包（需 .NET Framework 4 targeting pack，本机未装）
dotnet build .\DlcUnlockPatcher.csproj -c Release -p:ILRepack=false -o build\patcher >nul
if errorlevel 1 (
    echo patcher build failed
    exit /b 1
)

echo === [2/4] build native proxy ===
call build.bat >nul 2>&1
if errorlevel 1 (
    echo native build failed
    exit /b 1
)

echo === [3/4] assemble dist tree ===
if exist dist rmdir /s /q dist
mkdir dist\DlcUnlockPatcher
copy /y build\version.dll                     dist\ >nul
copy /y proxy.ini                             dist\ >nul
copy /y build\patcher\DlcUnlockPatcher.dll    dist\DlcUnlockPatcher\ >nul

echo === [4/4] pack zip ===
powershell -NoProfile -Command "Compress-Archive -Path dist\* -DestinationPath DlcUnlockPatcher-deploy.zip -Force"
if errorlevel 1 (
    echo pack failed
    exit /b 1
)

echo.
echo === dist tree ===
dir /s /b dist
echo.
echo packed: DlcUnlockPatcher-deploy.zip  (解压到游戏根目录即可)
endlocal
