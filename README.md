# Soft-ata Version 12.200

## Updates
- Added IoT Hub-Sensor-Telemtry example.
- Added another Blockly example that demos looking up devices and commands. Simple!
- New Blockly samples, removed old ones
  - 2 Samples that function same as Blazor2, 2 alternative Starts
    - One gets meta info from Database, other queries Pico.
  - Some new CategoriesLoaded samples
    - 2 x Menu functions (one withQuit) and Function to prompt for input and return single quoted string.
- Restored Telemetry to SoftataWebAPI **Works!** Can't selevct a sensor (simplest is simulator), configure for IoTHub abd Devices and start send telemetry and stop.
  - Can set and use confoig data in EEPROM  Start Options: Y, 2, Y   will bring you to entering;
    - SSID
    - PWD
    - Pico name/Hub name
    - Hub Device name
    - Device Connection string
    - Next time start options: Y, 2, N
- Also fix for Actuator 1 byte cmds, eg Servo sends byte via pin param
  - Single byte commands can use pin or as single byte array param.
- Major cleanup and restoration of Arduino Sketch [/code/Softata](/code/Softata) to repository
  - The SoftataLib for Blaozr or Console are same but the Blockly one is different becuase of Session issues.
    - Blaozr or Console use [/code/SofataLib](/code/SofataLib)
    - Blockly (SoftataWebAPI) uses [/code/SofataLibSessioned4Blockly](/code/SofataLibSessioned4Blockly)
      - Main differnce is that calls to Softata for Blockly (from ASP.Net Controllers) require the Client as a parameter that has to be reconnected to using connection details (IPAddress and Port) from Session each time. Can't put the Client into the session as I found out.
  - Still 2 Do:
    - Add Displays/Misc o Blockly
    - Restore Telemetry to Blockly
- Figured out a way to run 2 or more Blocklies in separate Browser tabs in Producer Consumer mode using sesssion.
- Blockly has been updaed for new Database
  - DeviceTypes,Device and Commands loaded from sqlite Database
     - Data that was previous read from RPi Pico W, this still can be done with an option.  See /Ard  blocks
       - /SoftataController Startup block queries the sqlite database
       - /Ard/SoftataController Startup block queries the Pico W Sketch
  - Database can be updated
  - Blocks are now Generic .. eg Instantiate a device by selecting iits device type then device etc.
- BlazorSoftata2 nearly complete
  - Meant to mimic SoftataConsole2
  - Introduces ILLayout that both apps implement, that is used by the C# library.
    - Console app uses the Nuget package
    - Blazor app has class at bottom of Index.razor
    - LATEST: Changing multiple inputs to one CSV line (ConsoleApp2 doe)
- SoftataConsole2 Added DeviceInput type. Implemented a switch (pins 16 to 21);
- SoftataConsole2 now works for Actuator Sensor and Display using ActionmSoftataCmd in SoftataLib rather than app.
  - Now for BlazorApp2 to use ActionmSoftataCmd, _Coming_
- SoftataConsole2 is the console app to use. Nb Blockly and Blazor apps are yet to be updated for this
  - The big change is to push evrything down into Arduino such that the .NET app can query for meta infor rather than have to know anything.
    - Can query for a list of Device Type classes, then ...
    - One assumption is that with Device Type classes, the first command (ndex 0) is GetCmds ... etc.
  - **KISS** principle: **K**eep it **S**ingle **S**ourced!
  - Watch this space
- Softata/src folder is now a separate GitHub repository at [djaus2/SoftataDevices](https://github.com/djaus2/SoftataDevices)
  - Much renaming there such as Grove becomes SoftataDevices
  - To run Softata sketch, install SoftataDevices library as a zip file
  - Much reworking needed of C# code.
- V11.400 In Console app, implemented Generic Display methods option ```ConsoleTestType.Displays_Generic:``` which calls methods common to all displays, even if not implemented. Should just ignore those.
  - getpins, getMiscCmds, clear, backlight, setCursor, writestrngCMD, cursor writestringCMD, homeCMD
  -   - Tested on LCD thus far (backlight is null for that).
- V11.300 As per 11.100 and 11.200  Individual misc commnds for LCD in Console app
  - Also fixed misc switch case miscode for LCD!
- V 11.200 As per 11.100 Individual misc commnds for GBargraph in Console app
  - ```ConsoleTestType.Displays_Suite_of_Tests```
  - viz ```ConsoleTestType.Displays_Suite_of_Tests```
  - Also added GBargraph misc commands in SoftataLib so can be directly used elsewehere.
- V 11.100 Extended Console App fuctionality
  - Can run fixed suite of tests for a display, as previous.
  - Or run indiviaul misc commands, as selected. Only implemented for Pixel display thus far and GBargraph (Grove LED Bar).
- V 11.000 Added sensor DHTXX that uses the DHTNew lib package available from lib in Arduino IDE. Can be used for DHT11,22 etc.
  - Includes auto adjusted min time btw reads.
    - Gets increased til no error. _(Starts at 1000mS and incr by 250mS. Seems that 2000mS works)_
    - If read is earlier than min time after last read, uses data from previous read.
    - Bonus: ```DHTXX.txt``` in BlocklyBonusExamplestoUpload folder: Upload in Blockly Is DHTXX sensor with 10 reads.
- V 10.900
  - Using Nuget package [ConsoleTextFormat](https://www.nuget.org/packages/ConsoleTextFormat) to color format Console text in the Console app.
    - [ConsoleTextFormat- GitHub](https://github.com/djaus2/ConsoleTextFormat)
- V10.199/200
  - Added Grove.Bargraph/Linksprite.bargraph as per [linksprite.bargraph.rpipico](https://github.com/djaus2/linksprite.bargraph.rpipico)
    - Added extra Misc commands now: flow,flow2,setLed,clrLed,toggleLed,setLevel,exercise
  - Console app: Can save settings (RPi Pico Server IPAddress and Port). Console app now has extra Misc coammnds as above and Grove.Bargraph option.
  - Renaming of tests in Console and Blazor apps
  - Renaming in Arduino Softata as well such as: Shift595ParaOut class to SIPO_74Hc595.
  - Console app now loops. When test finishes get menu again.
- V10.111 ***Actuator:Grove Relay added***
  - Connect to Pin 16(default)/18 or 20 (Socket 3/4 or 5 on bottom)
  - Can set,clear or toggle
  - _Note that bit parameter is ignored but required (use 0)._
- V10.110: ***Simplified C2D message protocol***
  - Can send C2D messages to Pause and Continue Telemetry
  - Eg "P1"  or "Pause 1"
  - First char in string P _ause_ or C _ontinue_ determines command. Case insensitive
  - Last char is index _(Nb: Typically 1 not 0)_.
  - T _oggle_, A _(Set)_, R _eset_ Actuator D2C messages coming.

---

## Index this page:


|   |   |
|:---|:-------|
| 1. [In brief](#in-brief)  | 8. [Soft-ata Projects](#soft-ata-projects)  |
| 2. [About](#about)  | 9. [RPi Pico W Arduino](#rpi-pico-w-arduino)  |
| 3. [History](#history) |  10. ["Some" of the required Arduino Libraries](#some-of-the-required-arduino-libraries)  |
| 4. [API Documentation](#api-documentation)  | 11. [Settings](#settings) |
| 5. [Local Docs](#local-docs) | 12. [Use](#use)   |
| 6. [Background](#background) | 13. [Blockly](#blockly) |
| 7. [Firmata](#firmata) | 14. [Azure IoT Hub](#azure-iot-hub)  |


## In Brief
**An Arduino sketch _LIKE_ Firmata for a Rapberry Pi  Pico W running Arduino.**
Includes a .NET package so that you can write your own remote client in C# to remotely control Pico devices. 
Console, Blazor and WebAPI app examples included with MAUI possible later. 

**A key objective was to also provide a Blockly style low code device programming using this infrastructure.**
  
_Soft-ata rather than firm-ata!_ 

> Branch 10.001 Is latest. Major resture of repository. All code under /code with a readme each. Under developement. Will merge back here when complete.


---

- Documentation:  [Related blog posts](https://davidjones.sportronics.com.au/cats/softata/)

- ***The Arduino Shield code is now downloadable as a Zip file when running the SoftataWebAPI***



## About

The Pico W app runs as a TCPIP Service taking commands, running them and returning the result to the client. 
For peripheral setups, displays and actuators, the expected result is simply an acknowledgement "OK:" string.
For sensor reads, a data string is returned with an "OK:" prefix. The SoftaLib checks and consumes the acknowledgment 
 data before it is forwarded to the client app.

The RPi Pico W has two processing cores. Whilst most interactions occur via the first core, 
some functionality is built into the second core. 
Firstly, the inbuilt LED flashes under control by the second core. 
When the Pico sketch first boots and both cores are ready, it blinks at a slow rate. 
_(4s on/4s off)_. Once a connection is made, it blinks at 4x this rate. 
  - The client app should not try connecting until then.
  - The device can run in a "headless" mode with status being indicated by "Coded flashes on the inbuilt LED" [on this page](https://davidjones.sportronics.com.au/softata/Softata-Arduino_Startup_Options-softata.html).
Communication between the two cores is generally from core one to core two and is done in a synchronized manner.

The second core is also used for autonomous streaming of Sensor Telemetry data over Bluetooth and to an Azure IoT Hub.
Once started, it runs with periodic transmissions without further interaction until a **Pause** or **Stop** command is sent.
When paused, the transmission continues after reception of a **Continue** command. 
For every transmission, there is also a quick double flash by the inbuilt LED.

Whereas Firmata is implemented from the ground up, implemented in terms of protocols with devices being added in terms of those implementations
Softata is implemented in terms of existing specific Arduino device libraries. 
To add a device you include its Arduino library and then slot it into the 
Softata app infrastructure. That code is, in the main, polymorphic. To add for example, a sensor
you copy you copy the template sensor.cpp file and implement the methods in it
according to the devices library samples. You also create header code for it largely by copying an existing sensor's header.
Within the Softata app and SoftataLib code there ar then some specific hooks to add for the device.
This process is documented [here](https://davidjones.sportronics.com.au/softata/Softata-Adding_a_new_device-softata.html). _This needs some updating._

## History

The plan was to implement an Arduino app to run on a [RPi Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html) placed in a [Grove Shield for Pi Pico](https://www.seeedstudio.com/Grove-Shield-for-Pi-Pico-v1-0-p-4846.html). 
The Pico W has onboard Arduino implemented using the [earlephilhowe BSP implementation](https://github.com/earlephilhower/arduino-pico). 
The Grove infrastructure being used because of it's simple standardised connectivity between devices and the shield at both ends.
Rather than implement a general purpose interface for devices in One Wire, I2C or SPI, etc, 
use is made of existing Arduino libraries for Grove devices.  

The intention was also to implement a class for each device type (sensor display, actuator etc) such
that the class can be extended for each actual device of that type by implementing the base methods. 
That way, the functionality of the app for a device type needs no modification for any additions. 
Additional non Grove devices can be added by connecting to a Grove cable.  

Ultimately the intention was to stream Telemetry from sensors to an Azure IoT Hub  
Communication with a host app using a client service model with the service running on the Arduino app connected to by clients running on a host. 
This is all now functional. 

A .NET library was built that communicates to the service as a mirror of the Arduino functionality. 
A Console app was built to fully test and demonstrate this functionality. A Blazor app to do same is under development. 
Similarly a .NET MAUI app is envisaged. Finally, a port of the .NET library to the [Wilderness Labs Project Lab V3 ](https://store.wildernesslabs.co/products/project-lab-board)device is also envisaged.***


## API Documentation

The full API documentation is [here](https://davidjones.sportronics.com.au/cats/softata/) __(Extended/updated)__

## Local Docs

- [ReadMe .. this](./README.md)
- [Softata API](./SoftataAPI.md)
- [Grove Sensors](./Grove.md)
- [Previous Docs](./PrevReadMesEtc)

## Background
I wanted to use a RPI Pico W with Arduino installed controlled by .NET apps. Drilling deeper, 
I wanted to make the Pico coding available with a Blockly style UI as per 
[CodeCraft](https://ide.tinkergen.com/). There is also [BlocklyDuino](https://blocklyduino.github.io/BlocklyDuino/). 

After some consideration Firmata was considered. But this lacks a simple .NET (not UWP) interface.

## Firmata

> Firmata is a protocol for communicating with microcontrollers from software on a host computer. The protocol can be implemented in firmware on any microcontroller architecture as well as software on any host computer software package. [From](https://github.com/firmata/arduino)

So if there a Firmata app running on a device, a host computer can interact directly with the device's hardware through a standard protocol over Serial, Ethernet (Wired or WiFi) or Bluetooth. The Firmata protocol can be viewed in the first link below. There are various implementations of it for various devices:
... There is more discussion of Firmata in the Blog post. ...

> I could get the [ConfigurableFirmata](https://github.com/firmata/ConfigurableFirmata) running on a RPi Pico W over WiFi. The.NET client libraries were quite old and used a Serial connection. Using a .NET TCPIP Client, I found that the functionality I could get working with interactively was limited. So I decided to build my own "Firmata", hence Soft-ata.

## Soft-ata Projects

- See [./code](./code)

## RPi Pico W Arduino

The Pico needs the following BSP installed:
- [earlephilhower RPi Pico W Arduino BSP](https://github.com/earlephilhower/arduino-pico/) 

Its API documentation is [here](https://arduino-pico.readthedocs.io/en/latest/)

This BSP is as per previous repositories here as well as in some blog posts:

- [RPI Pico W GPS Bluetooth and Azure IoT Hub](https://github.com/djaus2/RpiPicoWGPSandBT)
- [Azure IoT Hub Arduino Raspberry Pi Pico with Telemetry](https://github.com/djaus2/Azure_IoT_Hub_Arduino_RPI_Pico_Telemetry)
- [RPI-Pico-Arduino-AzSDK: Context](https://davidjones.sportronics.com.au/ardpico/RPI-Pico-Arduino-AzSDK-Context-pic-ard.html)


------


## The required Arduino Libraries

### Update 29 Oct 2024 

> _(Been reinstalling PC so checking these libraries)_

- ~~[BMx280BMI](https://bitbucket.org/christandlg/bmx280mi/src/master/)~~ ***<-- Updated 29 Oct /24***
  - ~~Direct library install from Arduino. Search for **BMx280BMI**~~
- [BME280](https://github.com/finitespace/BME280) by Tyler Glenn (finitespace)
  - Direct library install from Arduino. Search for BME280
- [Grove_Ultrasonic_Ranger](https://github.com/Seeed-Studio/Seeed_Arduino_UltrasonicRanger)
  - Zip file install from [here](https://github.com/Seeed-Studio/Seeed_Arduino_UltrasonicRanger/archive/master.zip)
- [u8g2](https://github.com/olikraus/u8g2) For [Grove OLED096 Display](https://wiki.seeedstudio.com/Grove-OLED_Display_0.96inch/)
  - Direct library install from Arduino. Search for **u8g2**  ***<-- Correction 29 Oct /24 8 not B***
- [Grove_LCD_RGB_Backlight](https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight) For Grove-LCD RGB V4.00
  - Zip file install from [here](https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight/archive/master.zip) 
- [Adafruit NeoPixel](https://github.com/adafruit/Adafruit_NeoPixel)
  - Direct library install from Arduino, Search for **Adafruit NeoPixel**
- [PubSubClient](https://pubsubclient.knolleary.net/)
  - Direct library install from Arduino. Search for **PubSubClient**
- [Azure SDK for C - Arduino](https://github.com/Azure/azure-sdk-for-c-arduino)
  - Direct library install from Arduino. Search for **Azure SDK for C** b Microsoft.
  - Zip install from [azure-sdk-for-c-arduino/releases](https://github.com/Azure/azure-sdk-for-c-arduino/releases)
- [Servo](https://docs.arduino.cc/libraries/servo/) by Michael Margolis
  - Direct library install from Arduino. Search for **Servo**
- **PS:** If you get **static_assert(ENABLE_CLASSIC** build error:
  - Tools->Bluetooth Stack set to Bluetooth and IPV4
  - Also need Flash Size 1M and 1M _(last option)_
- Re: DHTNew _(Watch this space)_
  - [DHTNew_Temperature_And_Humidity_Sensor](https://github.com/RobTillaart/DHTNEW)
    - Direct library install from Arduino. Search for **DHTNew**
  - Setup for the DHT11 (Maybe use DHTNew lib)?? Resolving this: _Will migrate to this library for DHTXX as this lib can sense wheich DHTXX and is in Arduino lib_)
    - Add this DHT11 library: _(Will be removed)_
      - The FrenoveStarter Kit for the Rpi Pico was used:
      - Clone the repoitory https://github.com/Freenove/Freenove_Ultimate_Starter_Kit_for_Raspberry_Pi_Pico (Get the zip file and then expand it.)
      - Open Arduino>Sketch>Include Library>Add .ZIP Library... ```Freenove_Ultimate_Starter_Kit_for_Raspberry_Pi_Pico-master\C\Libraries\DHT.zip```
------

## Settings

The Softata sketch requires at least, a WiFi SSID and Password. If sending telemetry to an Azure IoT Hub then connectivity settings for that are also required. There are a number of ways these settings can be ascribe such as fixed in a header file, input or read from flash. See [Arduino Startup Options](https://davidjones.sportronics.com.au/softata/Softata-Arduino_Startup_Options-softata.html). The flash method can be set and survive download of a new sketch.

## Use

See the [Console app](/code/SoftataConsole). The IpAddress as determined when the Pico W runs must match that in the library. The ports must also match.
The Console test app has multiple options:

```
1.      Digital Button and LED
2.      Analog Potentiometer and LED
3.      PWM
4.      Servo
5.      Sensors
6.      Displays
7.      Loopback
8.      Analog Potentiometer Light and Sound
9.      USonicRange
A.      Potentiometer and Actuator
B.      GPS Serial
C.      Test OTA Or WDT
D.      Analog Device Raw
```

The Blazor app has similar functionality.

## Blockly

[SoftataWebAPI](./code/SoftataWebAPI) is an ASP.NET Core API sketch that presents a Blockly and a Swagger interface to all of the SoftataLib (.NET) API commands. This makes use of [ignatandrei/NETCoreBlockly](https://github.com/ignatandrei/NETCoreBlockly). **SoftataWebAPI** can also be used without installation as it is hosted on Azure at [softatawebapii.azurewebsites.net](https://softatawebapii.azurewebsites.net/). Note though that some tunnelling is required to connect SoftataWebAPI and the Softata service running locally on a Pico W.  See [Running Blockly on local Pico from Azure Softata API](https://davidjones.sportronics.com.au/softata/Softata-Running_Blockly_on_local_Pico_from_Azure_SoftataAPII-softata.html). Eg:

![netcoreblockly](netcoreblockly-decluttered-large.png)

This Blockly app connects to the device at 192.168.0.9 (Begin), gets the version number of Softata _(currently 10.001)_, gets a list of devices that can be used, and completes _(which reboots the device)_. 

------

## Azure IoT Hub

- Telemetry can be sent from a sensor connected to the Pico to an Azure IoT Hub. See [Softata-Console_app_-_Sensors](https://davidjones.sportronics.com.au/softata/Softata-Console_app_-_Sensors-softata.html)Some CD messages can be sent to stop, pause and start such telemetry. Control of an actuator via CD Messages is a work in progress.

------

Enjoy! :)




