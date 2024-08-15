#ifndef NETWORK_SETTINGSH
#define NETWORK_SETTINGSH


// See serial_macros.h:
#define SOFTATA_DEBUG_MODE

#define WIFI_CONNECT_MODE from_eeprom
#define SERIAL_DEBUG true

//WiFi
#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "tallinn187isnotinaustralia"
#endif

#define IOT_CONFIG_WIFI_SSID STASSID
#define IOT_CONFIG_WIFI_PASSWORD STAPSK

#define IOT_CONFIG_HUBNAME "<hubname>"
#define IOT_CONFIG_DEVICE_ID  "<devicename>"
#define IOT_CONFIG_DEVICE_KEY  "<device connection key>"






#endif