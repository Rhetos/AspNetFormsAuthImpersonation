@SETLOCAL
@PUSHD "%~dp0"

@ECHO Target folder = [%1]
@ECHO $(ConfigurationName) = [%2]

XCOPY /Y/D/R Plugins\Plugins\AspNetFormsAuthImpersonation\bin\%2\Plugins\AspNetFormsAuthImpersonation.dll %1 || GOTO Error1
XCOPY /Y/D/R Plugins\Plugins\AspNetFormsAuthImpersonation\bin\%2\Plugins\AspNetFormsAuthImpersonation.pdb %1 || GOTO Error1

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
@EXIT /B 1
