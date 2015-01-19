@REM HINT: SET THE SECOND ARGUMENT TO /NOPAUSE WHEN AUTOMATING THE BUILD.
@SETLOCAL
@PUSHD "%~dp0"

@SET Config=%1%
@IF [%1] == [] SET Config=Debug

CALL InitRhetosSourcePath.bat /nopause || GOTO Error1

@REM Copy the package's Rhetos plugins.
@IF NOT EXIST Plugins\ForDeployment\ MD Plugins\ForDeployment\
@DEL /F /S /Q Plugins\ForDeployment\* || GOTO Error1
CALL CopyPlugins.bat Plugins\ForDeployment\ %Config% || GOTO Error1

"%RhetosSourcePath%\Source\CreatePackage\bin\%Config%\CreatePackage.exe" . || GOTO Error1

@RD /S /Q Plugins\ForDeployment\
@POPD

@REM ================================================
:Done
@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0
:Error1
@POPD
:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@IF /I [%2] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
