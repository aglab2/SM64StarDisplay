cd /d "D:\SM64StarManager\StarManager" &msbuild "StarManager.csproj" /t:sdvViewer /p:configuration="Debug" /p:platform=Any CPU
exit %errorlevel% 