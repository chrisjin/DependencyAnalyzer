:run.bat
:
:runs parser code
@echo off

echo   * If projects are not showed completely, please click refresh button.

echo   * Exceptions are written in the log.txt file.

echo   * Subtree option are tested in the command line mode.
 
echo   * File paths can all be specified either by config.xml or by command line.

echo ------Test------

pause

cd "./bin/server2"
start "" "DependencyAnalyzer.exe"  TestSample 



cd "../server1"
start "" "DependencyAnalyzer.exe" TestSample _NoSub_



cd ..
start client.exe

start client.exe
