@SETLOCAL
@REM ///////////////////////////
@SET NewVersion=1.3.0 alpha001
@REM \\\\\\\\\\\\\\\\\\\\\\\\\\\

@REM HINT: SET THE FIRST ARGUMENT TO /NOPAUSE WHEN AUTOMATING THE BUILD.
@PUSHD "%~dp0"

CALL InitRhetosSourcePath.bat /nopause || GOTO Error1
CALL "%RhetosSourcePath%\ChangeRhetosPackageVersion.bat" . %NewVersion% || GOTO Error1
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
@IF /I [%1] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
