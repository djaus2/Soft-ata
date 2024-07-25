@echo off 
cls
@echo USB-Serial uf2 deployment of (previously) built sketch uf2
@echo Expecting python3 to be in Path
@echo Expecting the built sketch uf2 in Sketch folder\ota
@echo Expecting the uf2conv.py from tools in ota folder
@echo Also expect pyserial folder from tools in ota folder
@echo eg tools folder: C:\Users\%USER%\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3\tools
@echo Remove device from USB, hold down BOOTSEL button, plug in USB, release BOOTSEL button.
@echo ===================================================

set sketchName=SoftataOTA
set sketchFolderLocation=C:\Users\%USERNAME%\source\Softata

@echo Sketch:          %sketchName%
@echo Sketch Folder:   %sketchFolderLocation%\%sketchName%
@echo serialPort:      uf2
@echo ===================================================
@echo .

pause


set pythonexe=python3.exe

set uf2convpyLocation=%sketchFolderLocation%\%sketchName%\OTA
set uf2convpy=uf2conv.py

REM Note: %sketchName%.ino.uf2 copied to <sketch foilder>\ota from <sketch folder>\build\rp2040.rp2040.rpipicow
REM Requires Sketch->Export Compiled Library build first.
set uf2Location=%sketchFolderLocation%\%sketchName%\OTA
set uf2file=%sketchName%.ino.uf2

REM If using directly from repository then <sketch foilder>\ota already has those files in place.

"%pythonexe%" -I "%uf2convpyLocation%\%uf2convpy%" --family RP2040 --deploy "%uf2Location%\%uf2file%"
pause