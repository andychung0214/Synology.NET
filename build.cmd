@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe Synology.NET.sln /p:Configuration=%config% /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:true /p:BuildInParallel=true /p:RestorePackages=true /t:Clean,Rebuild

if not "%errorlevel%"=="0" goto failure

rd Download /s /q  REM delete the old stuff

if not exist Download\Net4 mkdir Download\Net4\

copy LICENSE.txt Download\
copy SynologyClient\bin\Release\RestSharp.dll Download\net4\
copy SynologyClient\bin\Release\Synology.dll Download\net4\
copy readme.txt Download\

:success

REM use github status API to indicate commit compile success

exit 0

:failure

REM use github status API to indicate commit compile success

exit -1


