# Soft-ata

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

## About
<font color="purple">
_The plan was to implement an Arduino app to run on a RPi Pico W placed in a Grove RPi Pico Shield. 
The Grove instructure being used because of it's simple standardised connectivity between devices and the shield at both ends.
Rather than implement a general purpose interface for devices in One Wire, I2C or SPI, etc, 
make use of existing Arduino libraries for Grove devices. 
Also implement a base library for each device type (sensor display, actuator etc) such
that each can be extended for each device of thta type by implementing teh base methods. 
That way, the functionality of the app for a device type needs no modification for any additions. 
Additional non Grove devices can be added by connecting to a Grove cable. 
Ultimately the intention was to stream Telemetry from sensors to an Azure IoT Hub._
</font>

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
- Azure IoT Hub telemtry streaming
  - Uses the implementation on GitHub at [djaus2/Azure_IoT_Hub_Arduino_RPI_Pico_Telemetry](https://github.com/djaus2/Azure_IoT_Hub_Arduino_RPI_Pico_Telemetry)
- Restructed Display part of Console app.
- Improved Sensor UI in Console app
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




