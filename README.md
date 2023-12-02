# Softata

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

## Background
I wanted to use a RPI Pico W with Arduino installed controlled by .NET apps. Drilling deeper, I  wanted to use the Pico with a Blockly UI as per [CodeCraft](https://ide.tinkergen.com/). There is also [BlocklyDuino)](https://blocklyduino.github.io/BlocklyDuino/). But I found that with both, whilst the Arduino Uno is supported there, there is no support for the Pico. I then found [NETCoreBlockly](https://github.com/ignatandrei/netcoreblockly) on GitHub (as discussed by [Scott Hanselman](https://www.hanselman.com/blog/using-the-blockly-visual-programming-editor-to-call-a-net-core-webapi))  for general .NET Blockly programming and looked a way to extend it for the Pico. So the idea here is to create a .NET package that interfaces to the Pico over WiFi or Bluetooth that then can be integrated into a Blazor app running the .NET Blockly. 

 ![BlockduinoExample](codecraft1.png) | ![Blockyexamples2](blockyby2.png)  
**Some Blockly coding examples: _CodeCraft, BlocklyDuino and NoteCoreBlockly_**

After some consideration I had a back-to-the-future moment. What about Firmata? Alas ...


## Firmata



