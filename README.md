# Soft-ata Version 9.00

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C# to remotely control Pico devices. 
Console, Blazor and WebAPI app examples included with MAUI possible later..
  
_Soft-ata rather than firm-ata!_ 

---

- Documentation:  [here](https://davidjones.sportronics.com.au/cats/softata/)

- ***The Arduino Shield code is now downloadable as a Zip file when running the SoftataWebAPI***

## <font color="red">Hot Press</font>
- V9.30 WatchDog -OTA issue resolved. WatchDog now enabled with OTA.
- V9.22 Filename fix for the .ino file. 
  - Code improvements
    - Have tested OTA launch while console app running. OTA works fine.
- V9.20 Merged OTA into the Sketc
  - Sketch: SoftataOTA
  - Found issue with OTA upload if using WatchDog Timer.
    - Can optinally disable WatchDog. __(Disabled)__
    - WatchDog max update period is about 8s but this upload takes 14.3 seconds!
    - See [earlephilhower/arduino-pico/issues/2285](https://github.com/earlephilhower/arduino-pico/issues/2285)
- V9.00 Added SoftatOptSerial sketch that makes the serial debug messages optional.
- V8.00 Added SoftataMiscController class for miscellaneous Blockly Blocks
    - Simplify Blockly code by encapsulating functionality for what would be a complex set of blocks
     into a Get method that takes input/s, processes it and returns the result.. See [the example in this blog post](https://davidjones.sportronics.com.au/softata/Softata-CustomBlocksviaASP.NETController-softata.html)
- V7.99 Added Shift595ParaOut class for 74HC595. 8 or 16 bits out.
  - Added simple custom function app to samples. Deployed to Azure:
    - https://softatawebapii.azurewebsites.net/BlocklyAutomation
- V7.95 Abstracted  74HC595  Shift Reg fucntionality from LedBarGraph to separate class so can use as a Genral Purpose Demux. Actuator version coming.
- V7.00 Adding Blockly Softata Samples added to Blockly Examples Menu
  - V7.10 Blockly samples moved to ```SoftaWebAPI/wwwroot/BlocklyAutomation/assests/showUsage/demoBlocks```
  - This is so that they are directly available from Examples menu in Blockly.
  - V7.11/2 Added BME280 IoT Hub Telemetry Sample and Bluetooth Version.
    - Easily changed for other sensors: DHT11 and URange
  - V7.13 Added 2nd Neopixel example showing Functional usage.
- V6.83 Blockly Added
  - SoftaWebAPI app now starts with index page
    -  Can choose link to **BlocklyAutomation** or to **Swagger**
  - 11 simple sample Blockly apps in ```SoftaWebAPI/Samples folder```.
    - Load in Blockly Save Local->Load Blocks and browse to folder.
  - For info see [blog post](https://davidjones.sportronics.com.au/softata/Softata-Blockly____The_Holy_Grail-softata.html)
- V6.20  Can send Telemtry pause continue or stop  as C2D Msg
  - Eg **Telemetry 0 pause 0** to pause. 
  - Use Azure IoT Explorer  or VS Code etc to send msg to device
  - Also can send raw msg commands eg **240 0 9 0** to pause. 
    - 240 = Sensor(Telemetry) 9 = pause 10=continue.
    - Further details, take last/latest post (at bottom) [here](https://davidjones.sportronics.com.au/cats/softata/)
  - Can also send raw C2D msg commands to actuator (Servo)
    - eg ```242 0 5 0 1 90``` to move servo to 90 degress.
    - Need to init servo first though.

## About.

The Pico app runs as a TCPIP Service taking commands, running them and returning the result to the client. 
For setups, displays and actuators, the expected result is simply an acknowledgement "OK:" string.
For sensor reads, a data string is returned with an "OK:" prefix. The SoftaLib checks and consumes the acknowledgments 
 data before it is forwarded to the client app.

The RPi Pico W has two procressing cores. Whilst most interactions occur via the first core, 
some functionality is built into the second core. 
Firstly, the inbuilt LED flashes under control by the second core. 
When the Pico app first boots and both cores are ready, it blinks at a slow rate. 
_(4s on/4s off)_. Once a connection is made, it blinks at 4x this rate. So the client app should try connecting until then. 
Communication between the two cores is generally from core one to core two and is done in a synchronised manner.

The second core is also used for automous streaming of Sensor Telemetry data over Bluetooth and to an Azure IoT Hub.
Once started, it runs with periodic transmissions without futher interaction until a **Pause** or **Stop** command is sent.
When paused, the transmission continues after reception of a **Continue** command. 
For every transmission, there is also a quick double flash by the inbuilt LED.

Whereas Firmata is implemented form the ground up, implemented in terms of protocols with ddevices being added interms of those implementations
Softata is implemented in terms of existing spcific Arduino device libaries. 
To add a device you include its Arduino library and then slot it into the 
Softata app infrastructure. That code is, in the main, polymorphic. To add for example, a sensor
you copy you copy the template sensor.cpp file and implement the methods in it
according to the devices library samples. You also create header code for it largely by copying an existing sensor's header.
Within the Softata app and SoftataLib code there ar then some specific hooks to add for the device.
This process is documented [here](https://davidjones.sportronics.com.au/softata/Softata-Adding_a_new_device-softata.html). _This needs some updating._

## History

The plan was to implement an Arduino app to run on a [RPi Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html) placed in a [Grove Shield for Pi Pico](https://www.seeedstudio.com/Grove-Shield-for-Pi-Pico-v1-0-p-4846.html). 
The Pico W has onboard Arduino implemented using the [earlephilhowe BSP implementation](https://github.com/earlephilhower/arduino-pico). 
The Grove instructure being used because of it's simple standardised connectivity between devices and the shield at both ends.
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


# API Documentation

The full API documentation is [here](https://davidjones.sportronics.com.au/cats/softata/) __(Extended/updated)__

## Local Docs

- [ReadMe .. this](./README.md)
- [Softata API](./SoftataAPI.md)
- [Repository History](./RepositoryHistory.md)
- [Original ReadMe](./README_V1.md)
- [Second ReadMe version](./README_V2.md)

## Background
I wanted to use a RPI Pico W with Arduino installed controlled by .NET apps. Drilling deeper, 
I wanted to make the Pico coding available with a Blockly style UI as per 
[CodeCraft](https://ide.tinkergen.com/). There is also [BlocklyDuino](https://blocklyduino.github.io/BlocklyDuino/). 

After some consideration Firmata was considered. But this lacks a simple .NET (not UWP) interface.

## Firmata

> Firmata is a protocol for communicating with microcontrollers from software on a host computer. The protocol can be implemented in firmware on any microcontroller architecture as well as software on any host computer software package. [From](https://github.com/firmata/arduino)

So if there a Firmata app running on a device, a host computrer can interact directly with the device's hardware through a standard protocol over Serial, Ethernet (Wired or WiFi) or Bluetooth. The Firmata protocol can be viewed in the first link below. There are various implementations of it for various devices:
... There is more discussion of Firmata in the Blog post. ...

> I could get the [ConfigurableFirmata](https://github.com/firmata/ConfigurableFirmata) running on a RPi Pico W over WiFi. The.NET client libraries were quite old and used a Serial connection. Using a .NET Tcpip Client, I found that the functionality I could get working with interactively was limited. So I decided to build my own "Firmata", hence Soft-ata.

## Soft-ata Projects

- Softata: The Arduino RPI Pico app
- SoftataLib: The. NET Library
- SoftataConsole: A simple .NET Console demo app
- BlazorSoftata: Blazor Web Server App, some tests as Console app.
- SoftataWebAPI: Swagger interface to SoftataLib

## RPi Pico W Arduino

This requires a setup as per previous repositories here as well as in some blog posts:

- [RPI Pico W GPS Bluetooth and Azure IoT Hub](https://github.com/djaus2/RpiPicoWGPSandBT)
- [Azure IoT Hub Arduino Raspberry Pi Pico with Telemetry](https://github.com/djaus2/Azure_IoT_Hub_Arduino_RPI_Pico_Telemetry)
- [RPI-Pico-Arduino-AzSDK: Context](https://davidjones.sportronics.com.au/ardpico/RPI-Pico-Arduino-AzSDK-Context-pic-ard.html)


------

## Required Arduino Libraries

- [DHT11_Temperature_And_Humidity_Sensor](https://github.com/RobTillaart/Arduino/tree/master/libraries/DHTlib)
  - Direct library install from Arduino. Search for **DHTlib**
- [BMx280BMI](https://bitbucket.org/christandlg/bmx280mi/src/master/)
  - Direct library install from Arduino. Search for **BMx280BMI**
- [Grove_Ultrasonic_Ranger](https://github.com/Seeed-Studio/Seeed_Arduino_UltrasonicRanger)
  - Zip file install from [here](https://github.com/Seeed-Studio/Seeed_Arduino_UltrasonicRanger/archive/master.zip)
- [ubg2](https://github.com/olikraus/u8g2) For [Grove OLED096 Display](https://wiki.seeedstudio.com/Grove-OLED_Display_0.96inch/)
  - Direct library install from Arduino. Search for **ubg2**
- [Grove_LCD_RGB_Backlight](https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight) For Grove-LCD RGB V4.00
  - Zip file install from [here](https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight/archive/master.zip) 
- [Adafruit NeoPixel](https://github.com/adafruit/Adafruit_NeoPixel)
  - Direct library install from Arduino, Search for **Adafruit NeoPixel**
- [PubSubClient](https://pubsubclient.knolleary.net/)
  - Direct library install from Arduino. Search for **PubSubClient**
- [Azure SDK for C - Arduino](https://github.com/Azure/azure-sdk-for-c-arduino)
  - Direct library install from Arduino. Search for **Azure SDK for C** b Microsoft.
  - Zip install from [azure-sdk-for-c-arduino/releases](https://github.com/Azure/azure-sdk-for-c-arduino/releases)
------

## Usage

See the [Console app](/SoftataConsole) but the IpAddress as determined when the Pico W runs must match that in the library. The ports must also match.
The Console test app has multiple options:

- 1  Digital
- 2  Analog
- 3  PWM
- 4  Servo
- 5  Sensors
- 6  Displays
- 7  Serial
- 8  PotLightSoundAnalog
- 9  UltrasonicRange
- 10  PotRelay
- 11  PotServo

The Blazor app has similar functionality.

The SoftataWebAPI is an ASP.NET Core API app presents a Swagger interafce to all of the SoftataLib (.NET) API commands

------

## Roadmap

- Add more devices: __Please submit.__
- Azure IoT C2D commands (Control actuators etc)
- _Please leave suggestions in Issues or Discussions,thx_
- _More:_ See the Blog post _(See API Documentation link above)_

------

Enjoy! :)




