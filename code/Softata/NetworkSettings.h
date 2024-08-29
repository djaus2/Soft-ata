#ifndef NETWORK_SETTINGSH
#define NETWORK_SETTINGSH


// defined in serial_macros.h:
// #define SOFTATA_DEBUG_MODE

#define WIFI_CONNECT_MODE from_eeprom

#define SERIAL_DEBUG true

//WiFi
#ifndef STASSID
#define STASSID "<ssid>"
#define STAPSK "<password>"
#endif

#define IOT_CONFIG_WIFI_SSID STASSID
#define IOT_CONFIG_WIFI_PASSWORD STAPSK

#define IOT_NAME "<hubname>"
#define IOT_CONFIG_DEVICE_ID  "<devicename>"
#define IOT_CONFIG_DEVICE_KEY  "<device connection key>"

///////////////////////////////////////////////////////////////

#define IOT_PATH ".azure-devices.net"
#define IOT_CONFIG_HUBNAME "Blockly2.azure-devices.net" 
#define IOT_CONFIG_HUB_NAME IOT_NAME  IOT_PATH








#endif