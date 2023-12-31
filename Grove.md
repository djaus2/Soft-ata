# Grove sensors

> Assumes use of Grove RPi Pico Shield with the RPi Pico with the [earlephilhower-arduino-pico](https://github.com/earlephilhower/arduino-pico) setup on GitHub.

## API

- Implemented for DHT11, BME280
- Implemented Grove Sensor API:
  - String GetPins()/Pin Options
    - List of pins that can be used, such as 16,18,20 with DHT11, or required pins
  - String GetListofProperties() as csv list.
  - bool Setup(int[] settings)
    - Returns true if setup worked with the supplied settings, the first of which will be the pin/s to be used
    - If no settings, default settings used, which is/are noted in returned GetPins() string.
  - bool ReadAll(double[] values)
    - Returns all available properties in the supplied double array.
    - Eg Temperature and Humidity for DHT11
    - Eg Temperature, Humidity and Pressure for BME280
    - Retuns true, or false if failed.
  - double Read(enum property)
    - Return just the one nominated property value.
    - If failed return double max
  - Restart() , App Done,or Shutdown()
 
  > Coming: Grove API for Displays OLED 0.96"and LCD1602
