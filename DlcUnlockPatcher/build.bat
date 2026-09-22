@echo off
setlocal
cd /d "%~dp0"

rem VCVARS 可由 pack.bat 探测后经环境传入；单独运行时在此自行探测
if not defined VCVARS (
    for %%V in ("18\Community" "18\Professional" "18\Enterprise" "17\Community" "17\Professional" "17\Enterprise") do (
        if not defined VCVARS if exist "C:\Program Files\Microsoft Visual Studio\%%~V\VC\Auxiliary\Build\vcvars64.bat" (
            set "VCVARS=C:\Program Files\Microsoft Visual Studio\%%~V\VC\Auxiliary\Build\vcvars64.bat"
        )
    )
)
if not defined VCVARS (
    echo vcvars64 not found: need VS 2022/2026 with C++ workload
    exit /b 1
)

call "%VCVARS%" >nul
if errorlevel 1 (
    echo vcvars64 failed
    exit /b 1
)

if not exist build mkdir build
del /q build\* 2>nul

cl /nologo /LD /O2 /EHsc /std:c++17 /utf-8 /W3 ^
   src\dllmain.cpp src\bootstrap.cpp src\version_export.cpp src\ini_config.cpp ^
   /Fe:build\version.dll /Fo:build\ ^
   /link /DEF:src\version.def
if errorlevel 1 (
    echo build failed
    exit /b 1
)

echo.
echo === exports ===
dumpbin /nologo /exports build\version.dll
echo.
echo build ok: build\version.dll
endlocal
