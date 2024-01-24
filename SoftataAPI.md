# Soft-ata API

## In Brief
**An Arduino app _LIKE_ Firmata for RPI Pico W running Arduino.**
Includes a .NET package so that you can write your own client in C#. 
Console app included with Blazor and MAUI apps coming soon.
  
_Soft-ata rather than firm-ata!_ 

# API Documentation

[Full API documentation](https://davidjones.sportronics.com.au/cats/softata/) __(Extended/updated)__

## Local Docs

- [ReadMe](./REAME.md)
- [Softata API ... this](./SoftataAPI.md)
- [Repository History](./RepositoryHistory.md)
- [Original ReadMe](./OldREADME.md)

# Grove API

- Grove Sensors DHT11,BME280 and Ultrasonic Ranger implemented
- Grove Sensor API API (Arduino):
  - String GetSensors()
  - String GetPins()/Pin Options
  - String GetListofProperties()
  - bool Setup() //Default settings
  - bool Setup(int[] settings)
  - bool ReadAll(double[] values)
  - double Read(enum property)
  - String GetTelemetry(); 
  - Also can initiate BT Stream of telemetry, pause and contnue it.
    - Runs in second core

- Grove Displays FreeNove(Adafruit) Neopixel8 and LCD1602 implemented
    - Grove OLE096 coming
- Grove Display API (Arduino):
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

- Analog Devices API (Arduino)
  - Grove Potentiometer, Sound and Light Sensors
  - Specific API for these (in C# lib not Arduino)
    - InitAnalogDevicePins(RPiPicoMode) //groveShield or defaultMode
    - SetAnalogPin(device,pinNumber,maxValue) //device:potentiometer,light,sound
    - AnalogReadXXXSensor()  // No params
 
- Actuators API in SoftataLib (C#)
  - string[] GetActuators()  //Get names of actuators implemented (SERVO is only one thus far)
  - linkedListNo = SetupDefault(ActuatorDevice deviceType)  //Use defaults D16 and 540, 2400 for SERVO
  - linkedListNo = Setup(ActuatorDevice deviceType, byte pinNumber)
  - linkedListNo = Setup(ActuatorDevice deviceType, byte pinNumber, byte min=0, byte max=0) //2Do
  - ActuatorWrite(byte linkedListNo, byte value)

  - Other
    - Serial1 and Serial2, including GPS
    - Second core used for periodic streaming over Bluetooth

 > See https://github.com/djaus2/Soft-ata/blob/master/Grove.md







