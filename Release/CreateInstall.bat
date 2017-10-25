@echo off
rem ----------------------------------------------
rem Create a release installation. 
rem    %1 = Name of file to create.
rem    %2 = Name of .iss file.
rem ----------------------------------------------

rem ----- Set the %APSIM% variable based on the directory where this batch file is located
cd %~dp0..
set APSIM=%CD%
cd %APSIM%\Release

set InnoSetup="C:\Program Files (x86)\Inno Setup 5\ISCC.exe"

mkdir "%2"

%InnoSetup% /Q /O"%2" /F"%1" "%2.iss"

call "C:\CsiroSign.bat" %2\%1.exe

rem Now copy the releases to the right directory - with the revision number.
copy "%2\%1.exe" "C:\inetpub\wwwroot\Files\%1.exe"

rmdir /S /Q "%2"