@REM HINT: SET SECOND ARGUMENT TO /NOPAUSE WHEN AUTOMATING THE BUILD.
@SETLOCAL

@SET Config=%1%
@IF [%1] == [] SET Config=Debug

IF NOT DEFINED VisualStudioVersion CALL "%VS140COMNTOOLS%VsDevCmd.bat" || ECHO ERROR: Cannot find Visual Studio 2015, missing VS140COMNTOOLS variable. && GOTO Error0
@ECHO ON

NuGet restore || GOTO Error0
MSBuild /target:rebuild /p:Configuration=%Config% /verbosity:minimal /fileLogger || GOTO Error0
NuGet.exe pack -o .. || GOTO Error0

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@IF /I [%2] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
