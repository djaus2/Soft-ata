#ifndef OTALEDFLASH_H
#define OTALEDFLASH_H

// See serial_macros.h:
//#define SOFTATA_DEBUG_MODE

//WiFi
#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "tallinn187isnotinaustralia"
#endif

#define SHORTPULSE 500
#define ULTRA_SHORTPULSE 250
#define LONGPULSE 1000
#define EXTRA_LONGPULSE 2000

#define BOOTING_NUMFLASHES  5
#define BOOTING_PERIOD SHORTPULSE

#define WIFI_STARTED_NUMFLASHES  3
#define WIFI_STARTED_PERIOD LONGPULSE

#define OTA_ON_START_NUMFLASHES  4
#define OTA_ON_START_PERIOD ULTRA_SHORTPULSE

#define OTA_ON_END_NUMFLASHES  4
#define OTA_ON_END_PERIOD LONGPULSE

#define OTA_ON_ERROR_NUMFLASHES  6
#define OTA_ON_ERROR_PERIOD EXTRA_LONGPULSE

#define READY_NUMFLASHES  8
#define READY_PERIOD ULTRA_SHORTPULSE

#endif

