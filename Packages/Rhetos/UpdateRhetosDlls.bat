@REM HINT: SET THE FIRST ARGUMENT TO /NOPAUSE WHEN AUTOMATING THE BUILD.
@SETLOCAL
@PUSHD "%~dp0"

CALL ..\..\InitRhetosSourcePath.bat /nopause || GOTO Error1

DEL /Q /F "FileVersions.txt" || GOTO Error1
DEL /Q /F "*.dll" || GOTO Error1
DEL /Q /F "*.xml" || GOTO Error1
DEL /Q /F "*.pdb" || GOTO Error1

@CALL :SafeCopy Source\Rhetos\bin\Autofac.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos\bin\Autofac.Integration.Wcf.??? || GOTO Error1
@CALL :SafeCopy AspNetFormsAuth\Plugins\Rhetos.AspNetFormsAuth\bin\Debug\Rhetos.AspNetFormsAuth.??? || GOTO Error1
@CALL :SafeCopy CommonConcepts\Plugins\Rhetos.Dom.DefaultConcepts.Interfaces\bin\Debug\Rhetos.Dom.DefaultConcepts.Interfaces.??? || GOTO Error1
@CALL :SafeCopy CommonConcepts\Plugins\Rhetos.Dom.DefaultConcepts\bin\Debug\Rhetos.Dom.DefaultConcepts.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Dsl.Interfaces\bin\Debug\Rhetos.Dsl.Interfaces.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Extensibility\bin\Debug\Rhetos.Extensibility.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Extensibility.Interfaces\bin\Debug\Rhetos.Extensibility.Interfaces.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Interfaces\bin\Debug\Rhetos.Interfaces.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Logging.Interfaces\bin\Debug\Rhetos.Logging.Interfaces.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Processing.Interfaces\bin\Debug\Rhetos.Processing.Interfaces.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Security\bin\Debug\Rhetos.Security.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Security.Interfaces\bin\Debug\Rhetos.Security.Interfaces.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Utilities\bin\Debug\Rhetos.Utilities.??? || GOTO Error1
@CALL :SafeCopy Source\Rhetos.Web\bin\Debug\Rhetos.Web.??? || GOTO Error1

PowerShell.exe -Command "dir *.dll, *.exe | Format-List Name, Length, @{Name=\"LastWriteTime\"; Expression={$_.LastWriteTime.ToString(\"yyyy-MM-dd HH:mm:ss\")}}, @{Name=\"FileVersion\";Expression={$_.VersionInfo.FileVersion}} | Out-File FileVersions.txt -Width 1000 -Encoding UTF8"

@POPD
@Goto Done

:SafeCopy
@IF NOT EXIST "%RhetosSourcePath%\%1" ECHO. && ECHO ERROR: Missing "%RhetosSourcePath%\%1" && EXIT /B 1
XCOPY /Y/R "%RhetosSourcePath%\%1" . || EXIT /B 1
@EXIT /B 0

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
