@REM HINT: SET THE SECOND ARGUMENT TO /NOPAUSE WHEN AUTOMATING THE BUILD.
@SETLOCAL
@PUSHD "%~dp0"

@SET Config=%1%
@IF [%1] == [] SET Config=Debug

@IF DEFINED VisualStudioVersion GOTO SkipVcvarsall
@SET VSTOOLS=
@IF "%VS100COMNTOOLS%" NEQ "" SET VSTOOLS=%VS100COMNTOOLS%
@IF "%VS110COMNTOOLS%" NEQ "" SET VSTOOLS=%VS110COMNTOOLS%
@IF "%VS120COMNTOOLS%" NEQ "" SET VSTOOLS=%VS120COMNTOOLS%
CALL "%VSTOOLS%\..\..\VC\vcvarsall.bat" x86 || GOTO Error1
@ECHO ON
:SkipVcvarsall

CALL Packages\Rhetos\UpdateRhetosDlls.bat /nopause || GOTO Error1

IF EXIST Build.log DEL Build.log || GOTO Error1
DevEnv.com AspNetFormsAuthImpersonation.sln /rebuild %Config% /out Build.log || TYPE Build.log && GOTO Error1

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
