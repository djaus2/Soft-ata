
@echo off 
cls

@echo.
@echo  [1m[33mOTA Upload RPi Pico W image to device via Over-The-Air mode[0m
@echo ...............................................
@echo Requires Python3 in the Path
@echo ...............................................
@echo.
@echo 1 - [41mSoftata OTA with SerialDebug*[0m
@echo 2 - [42mSoftata OTA no SerialDebug[0m
@echo 3 - [41mOTA Test[0m
@echo 9 - Exit
@echo .

SET /P M=Type 1 (Default), 2 or 3 then press ENTER (9 to exit):
IF %M%1==1 GOTO SOFTATAwithSERIALDEBUG
IF %M%==1 GOTO SOFTATAwithSERIALDEBUG
IF %M%==2 GOTO SOFTATAnoSERIALDEBUG
IF %M%==3 GOTO OTATEST
IF %M%==9 GOTO ENDD

:SOFTATAwithSERIALDEBUG
cls
@echo   [1m[35mSoftataOTA WITH Serial Debug Messages[0m
@echo  Upload RPi Pico W image to device via Over-The-Air mode
@echo .
set sketchName=SoftataOTAwithSerialDebug
@echo Provision device with SoftataOTA using OTA
@echo Requires Serial Monitor to view debug messages.(Blocking)
GOTO RUNN

:SOFTATAnoSERIALDEBUG
cls
@echo   [1m[35mSoftataOTA with NO Serial Debug Messages[0m
@echo .
@echo  Upload RPi Pico W image to device via Over-The-Air mode
set sketchName=SoftataOTAnoSerialDebug
@echo Provision device with SoftataOTA over OTA
@echo No Serial Monitor required.(Non Blocking).
GOTO RUNN

:OTATEST
cls
@echo   [1m[35mOver-The-Air (OTA) Test[0m
@echo .
@echo  Upload RPi Pico W image to device via Over-The-Air mode
set sketchName=OTALedFlash
@echo Provision the device with OTA Test Sketch using OTA
@echo Requires Serial Monitor to view debug messages.(Blocking)
GOTO RUNN


:RUNN

set defaultipaddress="192.168.0.20"
set otaport="2040"


@echo Enter  ipaddress for device
@echo ===================================================

set /p ipaddress=What IPAddress? Default: %defaultipaddress%
if "%ipaddress%"=="" set ipaddress=%defaultipaddress%

set binfile=%sketchName%.ino.bin

set espota=espota.py


@echo Sketch:			%sketchName%
@echo BinFile			%binfile%
@echo Device IPAdress:	%ipaddress%
@echo OTA Port:			%otaport%
@echo ===================================================
@echo .



set pythonexe=python3.exe


@echo Command: "%pythonexe%" -I ".\%espota%" -i %ipaddress%  -p %otaport%  auth{upload.field.password} -f ".\%binfile%"

pause

"%pythonexe%" -I ".\%espota%" -i %ipaddress%  -p %otaport%  auth{upload.field.password} -f ".\%binfile%"

pause

:ENDD