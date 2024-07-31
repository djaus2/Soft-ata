## Misc Arduino Code

### OTALedFlash

An example Sketch for OTA (Over-The-Air) updating of a RPi Pico W.
Similar to examples at [https://github.com/earlephilhower/arduino-pico/tree/master/libraries/ArduinoOTA/examples](https://github.com/earlephilhower/arduino-pico/tree/master/libraries/ArduinoOTA/examples) especially the first 2. This one uses Inbuilt LED flashes to indicate where the sketch is at.

> Note: If device is disconnected to USB Serial then no Serial Debug messages show and hence flashing LeD is useful. 

Look at settings in OTALedFlash:

```c
#define SHORTPULSE 500
#define ULTRA_SHORTPULSE 250
#define LONGPULSE 1000
#define EXTRA_LONGPULSE 1000

#define BOOTING_NUMFLASHES  5
#define BOOTING_PERIOD SHORTPULSE

#define WIFI_STARTED_NUMFLASHES  3
#define WIFI_STARTED_PERIOD LONGPULSE

#define OTA_ON_START_NUMFLASHES  4
#define OTA_ON_START_PERIOD ULTRA_SHORTPULSE

#define OTA_ON_END_NUMFLASHES  4
#define OTA_ON_END_PERIOD LONGPULSE

#define OTA_ON_ERROR_NUMFLASHES  6
#define OTA_ON_ERROR__PERIOD EXTRA_LONGPULSE

#define READY_NUMFLASHES  8
#define READY_PERIOD ULTRA_SHORTPULSE

```