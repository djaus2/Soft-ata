@echo off 
cls

set sketchName=SoftataOTA
set sketchFolderLocation=C:\Users\david\source\Softata
set defaultipaddress="192.168.0.20"
set otaport="2040"

@echo OTA deploymengt of (previously) built sketch bin
@echo Expecting python3 to be in Path
@echo Expecting the built sketch bin in Sketch folder\ota
@echo Expecting the espota.py in ota folder from tools folder:
@echo eg tools folder: C:\Users\%USER%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3\tools
@echo Enter  ipaddress for device
@echo  ... Or edit in this batch file
@echo ===================================================

set /p ipaddress=What IPAddress? Default: %defaultipaddress%
if "%ipaddress%"=="" set ipaddress=%defaultipaddress%


@echo Sketch:          %sketchName%
@echo Sketch Folder:   %sketchFolderLocation%\%sketchName%
@echo Device IPAdress: %ipaddress%
@echo OTA Port:        %otaport%
@echo ===================================================
@echo .

pause


set USERAPPDataLocal=C:\Users\%USERNAME%\AppData\Local

set pythonexe=python3.exe

set espotaLocation=%sketchFolderLocation%\%sketchName%\OTA
set espota=espota.py

set binLocation=%sketchFolderLocation%\%sketchName%\OTA
set binfile=%sketchName%.ino.bin

"%pythonexe%" -I "%espotaLocation%\%espota%" -i %ipaddress%  -p %otaport%  auth{upload.field.password} -f "%binLocation%\%binfile%"

pause