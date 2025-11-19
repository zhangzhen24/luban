@echo off
chcp 65001 >nul
echo ========================================
echo 构建 Luban 并复制到指定目录
echo ========================================
echo.

set LUBAN_SRC_DIR=%~dp0src
set BUILD_CONFIG=Release
set TARGET_DIR=F:\Projects\EmberGuardian\trunk\Tools2\Exe\Luban

cd /d "%LUBAN_SRC_DIR%"

echo [1/3] 正在构建 Luban 解决方案...
dotnet build Luban.sln -c %BUILD_CONFIG% --no-incremental
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 构建失败！
    pause
    exit /b 1
)

echo.
echo [2/3] 正在复制文件到目标目录...
set OUTPUT_DIR=%LUBAN_SRC_DIR%\Luban\bin\%BUILD_CONFIG%\net8.0\

if not exist "%TARGET_DIR%" (
    echo 创建目标目录: %TARGET_DIR%
    mkdir "%TARGET_DIR%"
)

echo 复制 Luban.Core.dll...
copy /Y "%OUTPUT_DIR%Luban.Core.dll" "%TARGET_DIR%\" >nul
copy /Y "%OUTPUT_DIR%Luban.Core.pdb" "%TARGET_DIR%\" >nul 2>nul

echo 复制 Luban.dll...
copy /Y "%OUTPUT_DIR%Luban.dll" "%TARGET_DIR%\" >nul
copy /Y "%OUTPUT_DIR%Luban.pdb" "%TARGET_DIR%\" >nul 2>nul

echo 复制 Luban.AngelScript.dll...
copy /Y "%OUTPUT_DIR%Luban.AngelScript.dll" "%TARGET_DIR%\" >nul
copy /Y "%OUTPUT_DIR%Luban.AngelScript.pdb" "%TARGET_DIR%\" >nul 2>nul

echo 复制模板文件...
if not exist "%TARGET_DIR%\Templates" mkdir "%TARGET_DIR%\Templates"
xcopy /E /Y /I "%OUTPUT_DIR%Templates\angelscript-json" "%TARGET_DIR%\Templates\angelscript-json\" >nul 2>nul
xcopy /E /Y /I "%OUTPUT_DIR%Templates\common\as" "%TARGET_DIR%\Templates\common\as\" >nul 2>nul

echo.
echo [3/3] 完成！
echo.
echo ✅ 构建成功并已复制到: %TARGET_DIR%
echo.

pause

