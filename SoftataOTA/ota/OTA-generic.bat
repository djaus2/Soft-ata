@echo off 
cls

set sketchName=SoftataOTA
set sketchFolderLocation=C:\Users\david\source\Softata
set defaultipaddress="192.168.0.20"

set otaport="2040"


set pythonLocation=C:\Users\%USERNAME%\AppData\Local%\Microsoft\WindowsApps
set pythonexe=python3.exe

set espotaLocation=C:\Users\%USERNAME%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3\tools
set espota=espota.py

set binLocation=%sketchFolderLocation%\%sketchName%\build\rp2040.rp2040.rpipicow
set binfile=%sketchName%.ino.bin


@echo OTA deployment of (previously) built sketch bin
@echo Expecting python3 to be in %pythonLocation%
@echo Expecting the built sketch bin in %binLocation%
@echo Expecting the espota.py in %espotaLocation%
@echo Enter  ipaddress for device
@echo  ... Or edit in this batch file
@echo ===================================================

set /p ipaddress=What IPAddress? Default: %defaultipaddress%
if "%ipaddress%"=="" set ipaddress=%defaultipaddress%

@echo Sketch:          %sketchName%
@echo Sketch Folder:   %sketchFolderLocation%\%sketchName%
@echo Device IPAdress: %ipaddress%
@echo Port:            %port%
@echo ===================================================
@echo .

pause


"%pythonLocation%\%pythonexe%" -I "%espotaLocation%\%espota%" -i %ipaddress%  -p %otaport%  auth{upload.field.password} -f "%binLocation%\%binfile%"
pause