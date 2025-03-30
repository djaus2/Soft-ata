#ifndef NETWORK_SETTINGSH
#define NETWORK_SETTINGSH


// defined in serial_macros.h:
// #define SOFTATA_DEBUG_MODE

#define WIFI_CONNECT_MODE is_defined //from_eeprom

#define SERIAL_DEBUG true

//WiFi
#ifndef STASSID
#define STASSID "<Insert>"
#define STAPSK "<insert>"
#endif

#define IOT_CONFIG_WIFI_SSID STASSID
#define IOT_CONFIG_WIFI_PASSWORD STAPSK

// See //#define USINGIOTHUB in Softata.h
#define IOT_NAME "<Insert if using IOT Hub>" 
#define IOT_CONFIG_DEVICE_ID "<Insert if using IOT Hub>" 
#define IOT_CONFIG_DEVICE_KEY "<Insert if using IOT Hub>" 
// "<device connection key>"

///////////////////////////////////////////////////////////////

#define IOT_PATH ".azure-devices.net"
#define IOT_CONFIG_HUBNAME "Blobklhy3.azure-devices.net" 
#define IOT_CONFIG_HUB_NAME IOT_NAME  IOT_PATH








#endif