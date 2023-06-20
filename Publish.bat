@echo off

rem ------------------------------------
rem Build solution.
echo Building NotionPublisher solution...
dotnet build -c Release
rem ------------------------------------
rem Delete dev files.
echo Deleting dev files...
set target_ext=.pdb
set target_json=appsettings.Development.json
set npapi_rel_dir=.\RedmineApi\bin\Release\net6.0
set rmlder_rel_dir=.\RedmineLoader\bin\Release\net6.0
set ntpub_rel_dir=.\RedminePublisher\bin\Release\net6.0
set am_rel_dit=.\AlertManager\bin\Release\net6.0
del /q /s "%npapi_rel_dir%\*%target_ext%"
del /q /s "%npapi_rel_dir%\%target_json%"
del /q /s "%rmlder_rel_dir%\*%target_ext%"
del /q /s "%rmlder_rel_dir%\%target_json%"
del /q /s "%ntpub_rel_dir%\*%target_ext%"
del /q /s "%ntpub_rel_dir%\%target_json%"
del /q /s "%am_rel_dit%\*%target_ext%"
del /q /s "%am_rel_dit%\%target_json%"
rem ------------------------------------
rem Copy dockerfile to release directory.
echo Copying dockerfiles to release directories...
set npapi_dodckerfile=.\jenkins\RedmineApiDockerFile
set rmlder_dodckerfile=.\jenkins\RedmineLoaderDockerFile
set ntpub_dodckerfile=.\jenkins\RedminePublisherDockerFile
set am_dockerfile=.\jenkins\AlertManagerDockerFile
copy /y "%npapi_dodckerfile%" "%npapi_rel_dir%\Dockerfile"
copy /y "%rmlder_dodckerfile%" "%rmlder_rel_dir%\Dockerfile"
copy /y "%ntpub_dodckerfile%" "%ntpub_rel_dir%\Dockerfile"
copy /y "%am_dockerfile%" "%ntpub_rel_dir%\Dockerfile"
rem ------------------------------------
rem Update docker tags.
echo Updating version...
set versionfile=NotionPublisher.version
for /f "tokens=1-2 delims=." %%a in ('type "%versionfile%"') do (
    set major=%%a
    set minor=%%b
)
for /f "usebackq" %%i in (`git rev-list --count HEAD`) do set count=%%i
echo Updated version %major%.%minor%.%count%
echo %major%.%minor%.%count% > %versionfile%