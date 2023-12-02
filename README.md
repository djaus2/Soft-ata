# Soft-ata

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

## Background
I wanted to use a RPI Pico W with Arduino installed controlled by .NET apps. Drilling deeper, I wanted to make the Pico coding available to my grandkids with a Blockly style UI as per [CodeCraft](https://ide.tinkergen.com/). There is also [BlocklyDuino](https://blocklyduino.github.io/BlocklyDuino/). But I found that with both, whilst the Arduino Uno is supported there, there is no support for the Pico. I then found [NETCoreBlockly](https://github.com/ignatandrei/netcoreblockly) on GitHub (as discussed by [Scott Hanselman](https://www.hanselman.com/blog/using-the-blockly-visual-programming-editor-to-call-a-net-core-webapi)) for general .NET Blockly programming and looked for a way to extend it for the Pico. So the idea here is to create a .NET package that interfaces to the Pico over WiFi or Bluetooth that then can be integrated into a Blazor app running the .NET Blockly. 

 ![BlockduinoExample](codecraft1.png) | ![Blockyexamples2](blockyby2.png)  
**Some Blockly coding examples: _CodeCraft, BlocklyDuino and NetCoreBlockly_**

After some consideration I had a back-to-the-future moment. What about Firmata? Alas ...


## Firmata

> Firmata is a protocol for communicating with microcontrollers from software on a host computer. The protocol can be implemented in firmware on any microcontroller architecture as well as software on any host computer software package. [From](https://github.com/firmata/arduino)

So if there a Firmata app running on a device, a host computrer can interact directly with the device's hardware through a standard protocol over Serial, Ethernet (Wired or WiFi) or Bluetooth. THe Firmata protocol can be viewed in the first link below. There are various implementations of it for various devices:

- [Firmata Protocol Documentation](https://github.com/firmata/protocol)
- [Firmata for Arduino](https://github.com/firmata/arduino)
- [ConfigurableFirmata](https://github.com/firmata/ConfigurableFirmata)
- And there is the web app [Firmata Builder](http://firmatabuilder.com/])

There are also [Firmata Client libraries](https://github.com/firmata/arduino#firmata-client-libraries) for use on the host end.

Whilst I could get the [ConfigurableFirmata](https://github.com/firmata/ConfigurableFirmata) running on a RPi Pico W over WiFi, with a .NET TCPIP Client, I found that thge functionality I could working interactively was limited. So I decided to build my own "Firmata", hence Soft-ata.


