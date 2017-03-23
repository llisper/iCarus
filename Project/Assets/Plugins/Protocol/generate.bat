@echo off
cd /d "%~dp0"
call "%VS140COMNTOOLS%vsvars32.bat"

set sln=SharpFlatbuffers.sln
set dir=..\..\..\..\SharpFlatbuffers\
set cfg=Debug

msbuild /p:Configuration=%cfg% %dir%%sln%
if not %errorlevel%==0 (
    pause
) else (
    copy /y %dir%FbsGen\flatc.exe .\flatc.exe
    copy /y %dir%FbsGen\bin\%cfg%\FbsGen.exe .\FbsGen.exe
    copy /y %dir%FbsGen\bin\%cfg%\SharpFlatbuffers.dll .\SharpFlatbuffers.dll
    if not %errorlevel%==0 pause
)

.\FbsGen.exe --package Protocol
if not %errorlevel%==0 pause

copy /y output\Protocol.dll .\Protocol.dll
if not %errorlevel%==0 pause
rd /s /q output
if not %errorlevel%==0 pause
