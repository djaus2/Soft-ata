# Soft-ata

<hr/>
<h2>Project is being updated:</h2>

-  Nearly compolete Azure IOT hub in Blazor app 2Do
- V4.95 Mainly just code improvement. Neopixel test extended
- Inbuilt LED flash rate: Slow if not connected, fast if connected. _(Done)_
  - NB: This runs on the second core.
- Major rework for clarity with enums.
- Blazor App:
  - Change UI to start with test Categories selection _(Working)_
    - Some overlap of test types between categories.
  - Added Analog Pot-Sound-Sound sensors test _(Works)_
  - _Note:_ Software bargraphs in app for Analog data.
  - Sensors
      - The 3 sensors now available in Blazor app
        - Read and generate json string only
        - Telemetry to IoT Hub soon 
  - Displays
    - Neopixel and Grove LCD16x2 RGB Display work with default setiings.
    - V4.12 OLED096 now **FULLY** works. _Extended tests now_.
      - And parameters can now be sent (position radius etc.)
      - [u8g2 reference](https://github.com/olikraus/u8g2/wiki/u8g2reference)
      - V4.11 Softata checks if is can directly write Cursor and string  as one method
      ... or if it has to do as 2 methods. Uses ```grove_Display->CursorWriteStringAvailable()``` method to check.. 
      - **Hint:** I'm using the OLED display in [Grove Beginner Kit for Arduino](https://wiki.seeedstudio.com/Grove-Beginner-Kit-For-Arduino/) connected to Pico without removing it as it's I2C  
     _Just need to have kit powered and not using I2C._  
    **AND with the connecting Grove cable, DISCONNECT the RED power wire** to avoid power clashes between the devices.
    - Note also added Home method to Display API. _Return text to start of line._

<hr/>

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

## About

***The plan was to implement an Arduino app to run on a [RPi Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html) placed in a [Grove Shield for Pi Pico](https://www.seeedstudio.com/Grove-Shield-for-Pi-Pico-v1-0-p-4846.html).  
The Pico W has onboard Arduino implemented using the [earlephilhowe BSP implementation](https://github.com/earlephilhower/arduino-pico). 
The Grove instructure being used because of it's simple standardised connectivity between devices and the shield at both ends.
Rather than impelement a general purpose interface for devices in One Wire, I2C or SPI, etc, 
make use of existing Arduino libraries for Grove devices. 
Also implement a class for each device type (sensor display, actuator etc) such
that the class can be extended for each actual device of that type by implementing the base methods. 
That way, the functionality of the app for a device type needs no modification for any additions. 
Additional non Grove devices can be added by connecting to a Grove cable.   
Ultimately the intention was to stream Telemetry from sensors to an Azure IoT Hub  
Communication with a host app uses a client service model with the service running on the Arduino app connected to by clients running on a host. 
This is all now functional. 

A .NET library was built that communicates to the service as a mirror of the Arduino functionality. 
A Console app was built to fully test and demonstrate this functionality. A Blazor app to do same is under development. 
Similarly a .NET MAUI app is envisaged. Finally, a port of the .NET library to the [Wilderness Labs Project Lab V3 ](https://store.wildernesslabs.co/products/project-lab-board)device is also envisaged.***


# API Documentation

[Full API documentation](https://davidjones.sportronics.com.au/cats/softata/) __(Extended/updated)__

- Coming: How to add devices, and a call-to-arms!

## Local Docs

- [ReadMe .. this](./README.md)
- [Softata API](./SoftataAPI.md)
- [Repository History](./RepositoryHistory.md)
- [Original ReadMe](./OldREADME.md)

## Latest

_See **Repository Status** below._
- MQTT etc (and IoT turned off by default): Faster Blazor app start.
  - #define in Softata.h
- Extending Blazor app to match the Console app. Digital Button-LED,Analog Pot-LED, Pot-Relay and Pot-Servo tests migrated.  _More to come._
  - Added a custom Bar Graph for analog value display in Blazor app.
  - Also introducing default pin settings for Analog tests 
- Azure IoT Hub telemtry streaming
  - Uses the implementation on GitHub at [djaus2/Azure_IoT_Hub_Arduino_RPI_Pico_Telemetry](https://github.com/djaus2/Azure_IoT_Hub_Arduino_RPI_Pico_Telemetry)
- _More:_ See Repository Status

## Background
I wanted to use a RPI Pico W with Arduino installed controlled by .NET apps. Drilling deeper, 
I wanted to make the Pico coding available to my grandkids with a Blockly style UI as per 
[CodeCraft](https://ide.tinkergen.com/). There is also [BlocklyDuino](https://blocklyduino.github.io/BlocklyDuino/). 

After some consideration I had a back-to-the-future moment. What about Firmata? Alas ...


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
  - Zip install from (azure-sdk-for-c-arduino/releases)[https://github.com/Azure/azure-sdk-for-c-arduino/releases]
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

------

## Roadmap

- Completed
- _Please leave suggestions in Issues or Discussions,thx_
- _More:_ See the Blog post 

------

Enjoy! :)




