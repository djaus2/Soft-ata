# Soft-ata Repository History

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

# API Documentation

[Full API documentation](https://davidjones.sportronics.com.au/cats/softata/) __(Extended/updated)__

- Coming: How to add devices, and a call-to-arms!

## Local Docs

- [ReadMe](./REAME.md)
- [Softata API](./SoftataAPI.md)
- [Repository History ... this](./RepositoryHistory.md)
- [Original ReadMe](./OldREADME.md)

## Repository History
- Version 4.00
  - Can stream sensor telemetry to Azure IoT Hub.
    -  _Pause and continue now work.)_
- Version 3.20
  - Restructed Display part of Console app:  
More generic for if other displays are added.
- Version 3.15
  - Improved Senor UI in Console app
- Version 3.12
  - Improved menu driven Serial. _Includes a bug fix wrt setting Serial1vSerial2 TxRx_
    - Grove Serial GPS option, connect to Serial1 or Serial2
  - Also option to turn off status/debug messages in C#.
- Version 3.10
  - Code improvement for Telemetry
  - Can pause of continue BT Telemetry stream.
  - Sensor UI improved.
- Version 3.00
  - Added Telemetry to Sensors
    - Can send all properties as Json string
    - Can also stream this continously, once started over Bluetooth to Serial Bluetooth terminal.
    - _Also:_ Stream to Azure IoT Hub
- Version 2.50
  - Added a Servo and test of it in Console app
- Version 2.41
  - Added Potentiometer-Relay Test to Console app.
    - Also menu at start to select test.
  - Added Grove Ultrasonic Ranger sensor
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






