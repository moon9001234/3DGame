@echo off
set UNITY_EXE=C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe
set PROJECT_PATH=D:\3DGame
set BUILD_PATH=D:\3DGame\Builds\Windows\3DGame.exe
set LOG_PATH=D:\3DGame\Builds\Windows\build.log

if not exist "D:\3DGame\Builds\Windows" mkdir "D:\3DGame\Builds\Windows"

"%UNITY_EXE%" -quit -batchmode -nographics -projectPath "%PROJECT_PATH%" -executeMethod SideScrollerWindowsBuilder.BuildWindows64 -buildOutputPath "%BUILD_PATH%" -logFile "%LOG_PATH%"

if errorlevel 1 (
    echo Build failed. See %LOG_PATH%
    pause
    exit /b 1
)

echo Build succeeded: %BUILD_PATH%
pause
