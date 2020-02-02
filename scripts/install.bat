cd C:\DenonService\
DenonService.exe uninstall
del /q C:\DenonService\*
xcopy C:\projects\DenonApi\src\DenonService\Bin\Debug C:\DenonService\ /s /e
DenonService.exe install --autostart
DenonService.exe start
pause