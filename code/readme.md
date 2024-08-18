# Softata

## code

1. [Softata](./Softata)  
    - The RPi Pico W Arduino Sketch
    - Renamed from SoftataOTA
      - _Nb: Previous Softata sketch deleted._
    - Runs Arduino code that supports a TCPIP Service.
    - Supports classes for running attached senors, actuators etc that can be orchestrated via the service.
    - Also Azure IoT Hub and Bluetooth Telemetry options.
2. [SoftataLib](./SofataLib)  
    - The C# Class that can be used by .NET apps
    - Provides connection between apps and the Arduino sketch service.
3. Sample apps

  - Provide interaction with the RPi Pico W via SoftataLib:

    (i) [SoftataConsole](./SoftaConsole)
    (ii) [BlazorSoftata](./BlazorSoftata)
    (iii) [SoftataWebAPI](./SoftataWebAPI)

4. [MiscArduinoSketches](./MiscArduinoSketches)
    - See folder
5. [NetStandardSoftata](./NetStandardSoftata)
    - Port of SoftataLib to .NETStandard as used by MeadowLab
    - _Needs updates_
6. [MeadowLab](./MeadowLab)/[MeadowLabs.WindowsDesktop](./MeadowLabs.WindowsDesktop)
    - MeadowLabs app for Softata
    - _Needs updates_

## SoftataWebAPI

This is the main target of this repository. It provides a remote Blockly style of coding for interactions with the RPi Pico W. There is also a Swagger option.