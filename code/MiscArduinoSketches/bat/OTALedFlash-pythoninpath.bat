
@echo off 
cls

set repositoryFolder=C:\Users\%USERNAME%\source\Softata
set sketchName=OTALedFlash
set sketchFolderLocation=%repositoryFolder%\MiscArduinoSketches\%sketchName%
set defaultipaddress="192.168.0.16"
set otaport="2040"

set toolsDir=C:\Users\%USERNAME%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3\tools


@echo OTA deploymengt of (previously) built sketch bin
@echo Expecting python3 to be in Path
@echo Expecting the built sketch bin in Sketch folder\build\rp2040.rp2040.rpipicow
@echo Expecting the espota.py in RepositoryFolder\MiscArduinoSketches\bat, copied from tools folder:
@echo eg tools folder: C:\Users\%USERNAME%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.4\tools
@echo Enter  ipaddress for device
@echo  ... Or edit in this batch file
@echo ===================================================

set /p ipaddress=What IPAddress? Default: %defaultipaddress%
if "%ipaddress%"=="" set ipaddress=%defaultipaddress%

set binLocation= %sketchFolderLocation%\build\rp2040.rp2040.rpipicow
set binfile=%sketchName%.ino.bin

set espotaLocation=%repositoryFolder%\MiscArduinoSketches\bat
set espota=espota.py


@echo Sketch:			%sketchName%
@echo Sketch Folder:	%sketchFolderLocation%
@echo BinLocation:		%sketchFolderLocation%\build\rp2040.rp2040.rpipicow
@echo BinFile			%sketchFolderLocation%\build\rp2040.rp2040.rpipicow\%binfile%
@echo Device IPAdress:	%ipaddress%
@echo OTA Port:			%otaport%
@echo ===================================================
@echo .

pause


set pythonexe=python3.exe





%pythonexe%" -I "%espotaLocation%\%espota%" -i %ipaddress%  -p %otaport%  auth{upload.field.password} -f "%sketchFolderLocation%\build\rp2040.rp2040.rpipicow\%binfile%"

pause
