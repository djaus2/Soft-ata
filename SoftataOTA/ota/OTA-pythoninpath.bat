@echo off 
cls
@echo OTA deploymengt of (previously) built sketch bin
@echo Expecting python3 to be in Path
@echo Expecting the built sketch bin in Sketch folder\OTA
@echo Expecting the espota.py in Sketch folder\OTA
@echo ===================================================

set sketchName=SoftataOTA
set sketchFolderLocation=C:\Users\david\source\Softata
set ipaddress="192.168.0.20"
set port="2040"

@echo Sketch:          %sketchName%
@echo Sketch Folder:   %sketchFolderLocation%\%sketchName%
@echo Device IPAdress: %ipaddress%
@echo Port:            %port%
@echo ===================================================
@echo .


set USERAPPDataLocal=C:\Users\%USERNAME%\AppData\Local

set pythonexe=python3.exe

set espotaLocation=%sketchFolderLocation%\%sketchName%\OTA
set espota=espota.py

set binLocation=%sketchFolderLocation%\%sketchName%\OTA
set binfile=%sketchName%.ino.bin

rem python -I "C:\Users\david\AppData\Local\arduino15\packages\rp2040\hardware\rp2040\3.9.3/tools/espota.py" -i "192.168.0.20" -p "2040" "--rem auth{upload.field.password}" -f "C:\Users\david\source\Softata\SoftataOTA\build\rp2040.rp2040.rpipicow/SoftataOTA.ino.bin"

"%pythonexe%" -I "%espotaLocation%\%espota%" -i %ipaddress%  -p %port%  auth{upload.field.password} -f "%binLocation%\%binfile%"