@echo off
:compile.bat
:
devenv ./DependencyAnalyzer.sln /rebuild debug
pause
