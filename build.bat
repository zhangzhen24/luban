@echo off
chcp 65001 >nul
echo ========================================
echo 构建 Luban 项目
echo ========================================
echo.

set LUBAN_SRC_DIR=%~dp0src
set BUILD_CONFIG=Release

cd /d "%LUBAN_SRC_DIR%"

echo [1/2] 正在构建 Luban 解决方案...
dotnet build Luban.sln -c %BUILD_CONFIG% --no-incremental
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 构建失败！
    pause
    exit /b 1
)

echo.
echo [2/2] 构建完成！
echo.
echo 输出目录: %LUBAN_SRC_DIR%\Luban\bin\%BUILD_CONFIG%\net8.0\
echo.

pause

