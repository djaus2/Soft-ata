
@echo off 
cls
@echo Get WiFi IP Address of device
@echo Directly copy GetWifiIPAddress.ino.uf2 built bin to device
@echo .
@echo Action: Set device in UF2 mode
pause

REM For sketches in MiscArduinoSketches only need to change this:
set sketchName=GetWifiIPAddress

set defaultdrive=D:
set drive=d:

set /p drive=What drive? Default: %defaultdrive%
if "%drive%"=="" set drive=%defaultdrive%

set repositoryFolder=C:\Users\%USERNAME%\source\Softata
set sketchFolderLocation=%repositoryFolder%\MiscArduinoSketches\%sketchName%



set binLocation= %sketchFolderLocation%\build\rp2040.rp2040.rpipicow
set binfile=%sketchName%.ino.uf2


@echo Sketch:			%sketchName%
@echo Sketch Folder:	%sketchFolderLocation%
@echo BinLocation:		%sketchFolderLocation%\build\rp2040.rp2040.rpipicow
@echo BinFile			%sketchFolderLocation%\build\rp2040.rp2040.rpipicow\%binfile%
@echo ===================================================
@echo .

pause

copy %sketchFolderLocation%\build\rp2040.rp2040.rpipicow\%binfile% %drive%\ /Y
@echo done
@echo .
@echo Open Putty (or similar) to the device COM port
pause
