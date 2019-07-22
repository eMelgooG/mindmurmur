@echo off
REM cadams 2019-07-21:
REM libflam3.dll has a routine for printing the flame which depends on calling tmpfile():
REM https://github.com/scottdraves/flam3/blob/master/flam3.c#L1637

REM But unfortunately Windows has a crappy default which tries to write these temp files to the root directory. Most users cannot write to that directory. The workaround they put in is straight-up awful, they say it's not threadsafe:
REM https://github.com/scottdraves/flam3/commit/3274d1b51b721e388e9e038d1945412c450ae224

REM It's actually not even threadsafe, it depends on unlink() deleting the file, which does not happen in time for the next time the file is used by another flam3_print_to_string call, even though they all happen from one thread (I checked by printing the thread ID in python). Thus any time the interpolated flames temp file gets shorter than it was before, junk is left in the temp file and flam3 gives an error and skips that interpolated frame.

REM There are a few workarounds here, but I went with the simplest. To let tmpfile() work, this snippet wil run fractal fr0st as admin.
REM Source: https://stackoverflow.com/questions/1894967/how-to-request-administrator-access-inside-a-batch-file/10052222#10052222

:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
    IF "%PROCESSOR_ARCHITECTURE%" EQU "amd64" (
>nul 2>&1 "%SYSTEMROOT%\SysWOW64\cacls.exe" "%SYSTEMROOT%\SysWOW64\config\system"
) ELSE (
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
)

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params= %*
    echo UAC.ShellExecute "cmd.exe", "/c ""%~s0"" %params:"=""%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------

cd fr0st-master
python fr0st.py
pause
