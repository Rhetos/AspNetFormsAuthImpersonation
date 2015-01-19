@PUSHD "%~dp0"
@IF NOT EXIST RhetosSourcePath.txt ECHO. 2>RhetosSourcePath.txt
@IF EXIST RhetosSourcePath.txt SET /P RhetosSourcePath=<RhetosSourcePath.txt
@ECHO.
@IF "%RhetosSourcePath%" EQU "" ECHO ERROR: Please download Rhetos source and enter the source folder's full path in %~dp0RhetosSourcePath.txt && GOTO Error1
@IF NOT EXIST "%RhetosSourcePath%\Source\Rhetos.Utilities" ECHO ERROR: Invalid Rhetos source folder. Subfolder "%RhetosSourcePath%\Source\Rhetos.Utilities" does not exist, for example. Please download Rhetos source and enter the source folder's path in %~dp0RhetosSourcePath.txt && GOTO Error1
@IF NOT EXIST "%RhetosSourcePath%\Source\Rhetos.Utilities\bin\Debug\Rhetos.Utilities.dll" ECHO ERROR: Rhetos binaries are not available. Please build the Rhetos source (%RhetosSourcePath%) using Build.bat, or provide an alternative source path. && GOTO Error1
@POPD

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error1
@POPD
:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@IF /I [%1] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
