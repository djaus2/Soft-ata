# Soft-ata

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

# Grove API

- Grove Sensors DHT11 and BME280 implemented
- Grove Sensor API:
  - String GetSensors()
  - String GetPins()/Pin Options
  - String GetListofProperties()
  - bool Setup() //Default settings
  - bool Setup(int[] settings)
  - bool ReadAll(double[] values)
  - double Read(enum property)

- Grove Displays FreeNove(Adafruit) Neopixel8 and LCD1602 implemented
    - Grove OLE096 coming
- Grove Display API:
  - static String GetDisplays()
  - static String GetPins()
  - bool Setup();
  - bool Setup(byte * settings, byte numSettings);. // 2Do
  - bool Clear();
  - bool Misc(byte miscCmd, byte * data, byte length=0);
  - bool Backlight();
  - bool SetCursor(byte x, byte y);
  - bool WriteString(String msg);
  - bool WriteString(byte x, byte y, String msg)

- Analog Devices
  - Grove Potentiometer, Sound and Light Sensors
  - Specific API for these (in C# lib not Arduino)
    - InitAnalogDevicePins(RPiPicoMode) //groveShield or defaultMode
    - SetAnalogPin(device,pinNumber,maxValue) //device:
    - AnalogReadXXXSensor()  // No params

 > See https://github.com/djaus2/Soft-ata/blob/master/Grove.md

## Latest
- See **Repository Status** below.
  - Added specific Analog devices API as above.
  - Added 2  Displays and 2 Console test app
    -  FreeNove(Adafruit) Neopixle8
    -  Grove LCD RGB Backlight (LCD1602)
    - For Displays there is a Misc command with various sub commands
  
  - Grove Display API added as above 

## Blog Post

Discusion and more details in the blog post: [Soft-ata: A Simple Firmata with .NET](https://davidjones.sportronics.com.au/web/Soft-ata-A_Simple_Firmata_with_.NET-web.html)

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

## Repository Status
- Version 2.40
  - Added specific Analog devices API as above
- Version 2.30
  - Added 2 Displays and to Console test app
    - FreeNove(Adafruit) Neopixel8
      - Connect to D16 on Grove RPi Pico Shield S signal pin on Neopixel
        - i.e. to Yellow strand of Grove 4 wire cable)
        - And Vcc pin to Red strand, Gnd pin to Black strand
    - Grove LCD RGB Backlight (LCD1602)
      - [seeedstudio Grove-LCD_RGB_Backlight](https://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/)
      - Connect to I2C0 on Grove RPi Pico Shield using Grove 4 wire cable
    - For Displays there is a Misc command with various display specific sub commands.
    - Grove Display API 
- Version 2.02
  - Can easiliy switch btw RPI Pico W defaults and Grove Pico Shield in header
  - V2.03 Fixup for USART defines
- Version 2.01
  - Main app defines collected in softata.h
  - Also add get version and get list of device types.
- Version 2.00
  - Grove Sensors API as above
  - Console Test app tests Grove sensors. Not in Blazor app yet. 2Do.
- Version 1.65
  - Added Blazor Server app that runs the Digital LED-Button test (as per Console app).
    - Analog Test added (as per Console App).
    - Serial1 and Serial2 Test added (as per Console App). _(NB: Bug fixed in that with Serial2)_
    - Wishlist: A reconfigurable Blazor UI to action Softata functionality.
- Version 1.20
  - Serial1 and Serial2 implemented for byte and char read/writes
    - SoftataLib.Serial.serialSetup(byte pin, int baud, byte serialportNo)
      - _pin is the Tx pin (Rx = Tx+1), serialportNo = 1 or 2_
    - SoftataLib.Serial.serialGetChar(byte serialportNo)
    - SoftataLib.Serial.serialWriteChar(byte serialportNo, char value)
    - SoftataLib.Serial.serialGetByte(byte serialportNo)
    - SoftataLib.Serial.serialWriteByte(byte serialportNo, byte value)
  - Communication between client and Pico made more robust:
    - Length of byte array included as first byte rather than read until \n
- Version 1.10
  - All three projects work
  - Digital IO and **Analog/PWM** IO implemented, place holders for other functionality in the Arduino app
  - Digital IO
    - SoftataLib.Digital.SetPinMode(pin, SoftataLib.PinMode.DigitalInput/Output)
    - SoftataLib.Digital.SetPinState(pin, SoftataLib.PinState.HIGH/LOW)
    - SoftataLib.Digital.GetPinState(pin);
    - SoftataLib.Digital.TogglePinState(pin)
  - There is also some general commands. _See Blog_
  - Analog/PWM
    - SoftataLib.Analog.AnalogRead(pin)
    - SoftataLib.PWN.SetPWM(pin,value)
  - Interaction starts with:
    - SoftataLib.SendMessageCmd("Begin")
  - _More:_ See the Blog post

------

## Usage

See the [Console app](/SoftataConsole) but the IpAddress as determined when the Pico W runs must match that in the library. The ports must also match.
The test app has three options:
- Digital IO: Button to LED
- Analog/PWM: Potentiometer sets LED brightness.
- Serial1 and Serial2 loopback tests.

------

## Roadmap

- Implement the other capabilities in the Arduino app, etc
- Azure IoT Hub connectivity
- _More:_ See the Blog post 

------

Enjoy! :)




