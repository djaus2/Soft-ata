
@echo off 
cls

@echo.
@echo  [1m[33mCopy RPi Pico W image to device via USB Serial in Uf2 mode[0m
@echo ...............................................
@echo Is direct file copy only. No requirements
@echo ...............................................
@echo.
@echo 1 - [41mSoftata OTA with SerialDebug*[0m
@echo 2 - [42mSoftata OTA no SerialDebug[0m
@echo 3 - [41mOTA Test[0m
@echo 4 - [42mFlash Wifi Details[0m
@echo 5 - [41mGet Wifi IPAddress[0m
@echo 9 - Exit
@echo .

SET /P M=Type 1 (Default), 2, 3, 4 or 5 then press ENTER (9 to exit):
IF %M%1==1 GOTO SOFTATAwithSERIALDEBUG
IF %M%==1 GOTO SOFTATAwithSERIALDEBUG
IF %M%==2 GOTO SOFTATAnoSERIALDEBUG
IF %M%==3 GOTO OTATEST
IF %M%==4 GOTO FLASHWIFI
IF %M%==5 GOTO WIFITEST
IF %M%==9 GOTO ENDD

:SOFTATAwithSERIALDEBUG
cls
@echo  [1m[33mCopy RPi Pico W image to device via USB Serial in Uf2 mode[0m
@echo ..............................................
@echo   [1m[36mSoftataOTA WITH Serial Debug Messages[0m
@echo .
set sketchName=SoftataOTAwithSerialDebug
@echo Provision device with SoftataOTA using uf2
@echo Requires Serial Monitor to view debug messages.(Blocking)
@echo Can follow up with OTA upload of this or next option.
GOTO RUNN

:SOFTATAnoSERIALDEBUG
cls
@echo  [1m[33mCopy RPi Pico W image to device via USB Serial in Uf2 mode[0m
@echo ..............................................
@echo   [1m[36mSoftataOTA with NO Serial Debug Messages[0m
@echo .
set sketchName=SoftataOTAnoSerialDebug
@echo Provision device with SoftataOTA using uf2
@echo No Serial Monitor required.(Non Blocking)
@echo Can follow up with OTA upload of this or previous option.
GOTO RUNN

:OTATEST
cls
@echo  [1m[33mCopy RPi Pico W image to device via USB Serial in Uf2 mode[0m
@echo ..............................................
@echo   [1m[36mOver-The-Air (OTA) Test[0m
@echo .
set sketchName=OTALedFlash
@echo Provision the device with OTA Test Sketch using Uf2
@echo Follow up with OTA upload of this.
GOTO RUNN

:FLASHWIFI
cls
@echo  [1m[33mCopy RPi Pico W image to device via USB Serial in Uf2 mode[0m
@echo ..............................................
@echo  [1m[36mFlash the EEProm with WiFi credentials.[0m
@echo .
set sketchName=eepromwifi
@echo Credentials are entered.
@echo SotataOTA will use them rather than the hardcoded ones or prompt for them.
GOTO RUNN

:WIFITEST
cls
@echo  [1m[33mCopy RPi Pico W image to device via USB Serial in Uf2 mode[0m
@echo ..............................................
@echo  [1m[36mGet device WiFi address, by entering credentials.[0m
@echo .
set sketchName=GetWifiIPAddress
@echo Get WiFi IP Address of device
GOTO RUNN

:RUNN
@echo .
@echo [1m[31mDirectly copy %sketchname%.ino.uf2 built bin to device[0m
@echo Disconnect usb cable and press BootSel on device and hold
@echo Reconnect cable and then release BootSel button.
@echo Note the assigned drive letter.
pause



set defaultdrive=d:
set drive=d:
set /p drive=What drive letter? Default: %defaultdrive%
if "%drive%"=="" set drive=%defaultdrive%

if exist %drive% ( GOTO NEXT ) 
if exist %drive%: ( 
	set drive=%drive%:
	GOTO NEXT 
)

@echo %drive% NOT Found
GOTO ENDD

:NEXT

set binLocation= .
set binfile=%sketchName%.ino.uf2


@echo Sketch:			%sketchName%
@echo BinFile			.\%binfile%
@echo ===================================================
@echo Copying to %drive%
@echo .

@echo [1m[33mCommand: copy .\%binfile% %drive% /Y[0m
pause
@echo .


copy .\%binfile% %drive% /Y

@echo done
@echo .
@echo Open Putty (or similar) to the device COM port
pause

:ENDD
