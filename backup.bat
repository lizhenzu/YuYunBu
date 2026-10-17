@echo off
set "YMD=%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%"
set "H=%TIME:~0,2%"
if "%H:~0,1%"==" " set "H=0%H:~1,1%"
set "M=%TIME:~3,2%"
set "S=%TIME:~6,2%"
set "STAMP=%YMD%_%H%%M%%S%"

echo %STAMP%
7z a "YunYunBu2026.%STAMP%.zip" -xr!*.zip -xr!.config -xr!.vs -xr!bin -xr!obj -xr!wwwroot *
