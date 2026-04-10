@echo off
echo ==============================================
echo       正在清除《DreamGraduation》游戏存档...
echo ==============================================
echo.

rem 防止由于不具备管理员权限导致无法删除
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [信息] 管理员权限确认成功。
) else (
    echo [提示] 建议右键选择 "以管理员身份运行" 确保清除彻底。
)

rem 删除整个游戏的注册表项
reg delete "HKEY_CURRENT_USER\Software\Unity\UnityEditor\DefaultCompany\DreamGraduation" /f >nul 2>&1

if %errorLevel% == 0 (
    echo.
    echo [成功] 存档已彻底清除！游戏现已恢复至初始状态。
) else (
    echo.
    echo [提示] 没有找到存档注册表项，或者存档已经被清除过了。
)

echo.
pause
