@echo off
::
:: Build script for "<insert app name>"
::
:: Copyright (C)2021-2023 Scott Henshaw, All rights reserved
:: USAGE:
::   AutoBuild [--nodebug] [--nopull] [--release:no|network|only] [--logonly] [ProjectName]
::
SETLOCAL ENABLEEXTENSIONS

:: Parse Command Line Options
set OPT-DEBUG=true
if /I "%1"==""          goto :usage

:getopts
if /I "%1"=="-h"            goto :usage
if /I "%1"=="--help"        goto :usage
if /I "%1"=="--logonly"     set OPT-LOGONLY=true & shift
if /I "%1"=="--nodebug"     set OPT-DEBUG=false & shift
if /I "%1"=="--nopull"      set OPT-NOPULL=true & shift
if /I "%1"=="--release:no"      set OPT-NOREL=true & shift
if /I "%1"=="--release:network" set OPT-NETREL=true & shift
if /I "%1"=="--release:only"    set OPT-RELEASE=true & shift
shift
if not "%1" == "" goto getopts
if DEFINED OPT-DEBUG (
    @echo Debug: %OPT-DEBUG%
    @echo NoPull: %OPT-NOPULL%
    @echo NoRelease: %OPT-NOREL%
)
goto :start

::--------------------------------------------------------------------------
:usage
::
:: AutoBuild
@echo USAGE:
@echo   AutoBuild [--nodebug] [--nopull] [--release:no^|network^|only] [--logonly] [ProjectName]
goto end


::--------------------------------------------------------------------------
:start
    :: generate timestamp
    call :SetNow now
    call :SetDate today
    set TIMESTAMP=%today%@%now%
    if DEFINED OPT-DEBUG (
        @echo Build started at: %TIMESTAMP%
    )

    ::----------------------------------------------------------
    :: The environment  -- EDIT THESE
    ::
    :: set PROJECT to your uproject file name without the .uproject extension
    ::
    ::set COHORT=<insert cohort name>
    ::set PROJECT=<project-name>
    set COHORT=GD65
    set PROJECT=ProjectName

    :: GIT:  Edit the line below to set your git repo if you use the clone option
    :: edit your URL
    set GIT_REPO="https://github.com/vfs-sct/%COHORT%FP-%PROJECT%.git"

    :: Edit these paths to where they exist on your build computer
    set UNITY="C:\Program Files\Unity\Hub\Editor\2021.3.7f1\Editor\Unity.exe"
    set UNREAL="C:\Program Files\Epic Games\UE_4.27\Engine"

    :: uncomment ONE of these
    :: set USE-ENGINE-UNREAL=unreal
    set USE-ENGINE-UNITY=unity

::  DONT TOUCH ANYTHING BELOW HERE
::----------------------------------------------------------

    :: SRC Folder, a folder to use for a clean build,
    :: this gets the full working path from the current working folder
    for %%d in (.) do (
        set SRC=%%~fd
    )

    :: DEST Folder is where the final build goes, after all things not required
    set DEST=%SRC%\Build
    if not exist %DEST%\ mkdir %DEST%

    :: This is where your build goes.
    if DEFINED OPT-NETREL goto :netrelease
    set RELEASE=%SRC%\Release
    goto :makerelease

::--------------------------------------------------------------------------
:netrelease
    set RELEASE="\\vfsstorage10\dropbox\GDPGSD\Builds\%COHORT%\%PROJECT%\Builds"

::--------------------------------------------------------------------------
:makerelease
    if not exist %RELEASE%\ mkdir %RELEASE%

    :: This identifies a log file for every build, what happened is listed
    :: if a build doesn't work, find out why, start here.
    if not exist AutoBuildLogs\ mkdir AutoBuildLogs
    set LOGFILE=%SRC%\AutoBuildLogs\Build-%TIMESTAMP%.log

    :: Start Logging
    call :SetNow now
    @echo %PROJECT% > %LOGFILE%
    call :TitleLog "%PROJECT% Build Started at %now%"

    set UCC=%UNREAL%\Binaries\DotNET\UnrealBuildTool.exe
    set UNREAL_PRE=%UNREAL%\Build\BatchFiles\RunUAT

    if DEFINED OPT-LOGONLY goto :gitlog
    if DEFINED OPT-RELEASE goto :releaseonly

    :: MAIN - Start the Build Process Here!
    @echo Building from: %SRC%  >>%LOGFILE%
    @echo Building to:   %DEST%  >>%LOGFILE%
    @echo Release to:    %RELEASE%  >>%LOGFILE%
    @echo -------------------------------------------- >>%LOGFILE%
    if DEFINED OPT-DEBUG (
        @echo Building from: %SRC%
        @echo Building to:   %DEST%
        @echo Release to:    %RELEASE%
        @echo --------------------------------------------
    )

::--------------------------------------------------------------------------
:git
    :: Get the Source control setup
    :: go get a fresh copy of your repo - main branch
    if DEFINED OPT-CLONE (
        git clone --branch develop %GIT_REPO%
    ) else (
        if NOT DEFINED OPT-NOPULL (
            git switch develop  >>%LOGFILE%
            git pull            >>%LOGFILE%
        )
    )

::--------------------------------------------------------------------------
:gitlog
    :: Generate git log from Date to Date
    set GIT_SINCE="yesterday"

    call :TitleLog "Release Notes ^- Included changes since: %GIT_SINCE%"
    set LOG_GIT=git --no-pager log
    set LOG__GIT_OPTS=--remotes="bitbucket" --branches="develop"
    set LOG_GIT_FMT=--pretty=format:"%%C(auto)%%h%%d (%%cr) %%cn <%%ce> %%s"
    set GIT_LOG_PARAMS=%LOG_GIT_OPTS% --since=%GIT_SINCE% %LOG_GIT_FMT%
    %LOG_GIT% %GIT_LOG_PARAMS% >>%LOGFILE%

if DEFINED OPT-LOGONLY goto :end

:: -------------------------------------------------------
:build
    :: Build the actual game
    call :TitleLog "Generating game"

    @echo "Errors here are because your game does not build in your engine!"
    if DEFINED USE-ENGINE-UNITY (
        :: UNITY
        :: Build the code with UNITY, comment the lines above
        @echo:
        call :ShortLog "Starting %UNITY% at %time:~0,8%"

        :: Build the code with Unity, comment out the unreal build and uncomment this
        start /b/wait "auto-building" %UNITY% -projectPath %SRC% -quit -batchmode -buildWindows64Player %DEST%\%PROJECT%.exe -logFile >>%LOGFILE%
        if exist %DEST%\%PROJECT%_BurstDebugInformation_DoNotShip (
            rmdir /s/q %DEST%\%PROJECT%_BurstDebugInformation_DoNotShip
        )
    )

    if DEFINED USE-ENGINE-UNREAL (
        :: UNREAL
        :: Build the code with UNREAL, uncomment the lines below
        @echo:
        call :ShortLog "Starting %UNITY% at %time:~0,8%"
        %UCC% %PROJECT% -ModuleWithSuffix %PROJECT% 5202 Win64 Development -editorrecompile -canskiplink %SRC%\%PROJECT%.uproject >>%LOGFILE%

        @echo:
        @echo Starting %UNREAL_PRE% at %time:~0,8% >>%LOGFILE%
        call %UNREAL_PRE% BuildCookRun -project=%SRC%\%PROJECT%.uproject -noP4 -platform=Win64 -clientconfig=Shipping -serverconfig=Shipping -cook -allmaps -build -stage -pak -archive -archivedirectory=%DEST% >>%LOGFILE%
    )

    call :ShortLog "Compile complete"

::--------------------------------------------------------------------------
:releaseonly

    :: Generate the release
    call :TitleLog "Generating nightly release"

    call :ShortLog "cleaning folders"

    :: Clean the oldest folders
    if DEFINED OPT-DEBUG @echo Cleaning %RELEASE%\oldest
    if exist %RELEASE%\oldest rmdir /S /Q %RELEASE%\oldest >>%LOGFILE%
    if DEFINED OPT-DEBUG @echo Moving %RELEASE%\yesterday build to oldest
    if exist %RELEASE%\yesterday ( move /Y %RELEASE%\yesterday %RELEASE%\oldest >>%LOGFILE% )
    if DEFINED OPT-DEBUG @echo Moving %RELEASE%\today to yesterday
    if exist %RELEASE%\today     ( move /Y %RELEASE%\today %RELEASE%\yesterday >>%LOGFILE% )

::--------------------------------------------------------------------------
:release-network

    :: Migrate the build to the release folder
    call :ShortLog "migrating nightly to %RELEASE%"

    if DEFINED USE-ENGINE-UNITY (
        xcopy /S /I /Q /Y /F %DEST% %RELEASE%\today                 >>%LOGFILE%
    )
    if DEFINED USE-ENGINE-UNREAL (
        xcopy /S /I /Q /Y /F %DEST%\WindowsNoEditor %RELEASE%\today >>%LOGFILE%
    )
    call :ShortLog "Test build is here:    %TEST%"
    call :ShortLog "Release build is here: %RELEASE%"

    :: Create zip archive
    call :ShortLog "Zipping build at %RELEASE%"
    zip -r -q %RELEASE%\%PROJECT%-%TIMESTAMP%.zip %RELEASE%\today >>%LOGFILE%

goto :end

::--------------------------------------------------------------------------
:: call :SetDate today
:SetDate
    ::set %~1=%date:~4,2%-%date:~7,2%-%date:~9,2%
    for /f "tokens=1-3 delims=//" %%a in ('date /T') do (
        set year=%%c
        set day=%%a

        set %~1=%year:~0,4%-%%b-%day:~4,2%
    )
EXIT /B 0

::--------------------------------------------------------------------------
:: call :SetNow now
:SetNow
    ::set %~1=%time:~0,2%.%time:~3,2%.%time:~6,2%
    for /f "tokens=1-2 delims=/:" %%a in ('time /T') do (
        set min=%%b

        set %~1=%%a.%min:~0,2%
    )
EXIT /B 0

::--------------------------------------------------------------------------
:: call :LogEntry title, subtitle
:TitleLog
    @echo: & echo:                                     >>%LOGFILE%
    @echo -------------------------------------------- >>%LOGFILE%
    @echo %~1 %~2                                      >>%LOGFILE%
    @echo:                                             >>%LOGFILE%
    if DEFINED OPT-DEBUG (
        @echo --------------------------------------------
        @echo %~1 %~2
        @echo:
    )
EXIT /B 0

::--------------------------------------------------------------------------
:: call :ShortLog title
:ShortLog
    @echo:    >>%LOGFILE%
    @echo %~1 >>%LOGFILE%
    if NOT DEFINED OPT-DEBUG (
        @echo:
        @echo %~1
    )
EXIT /B 0

::--------------------------------------------------------------------------
:end
    call :TitleLog "Build Complete."

:: Unset all the temp env variables we used
ENDLOCAL
