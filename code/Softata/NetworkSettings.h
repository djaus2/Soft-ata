#ifndef NETWORK_SETTINGSH
#define NETWORK_SETTINGSH


// defined in serial_macros.h:
// #define SOFTATA_DEBUG_MODE

#define WIFI_CONNECT_MODE is_defined //from_eeprom

#define SERIAL_DEBUG true

//WiFi
#ifndef STASSID
#define STASSID "OPTUS_EE3AEDM"
#define STAPSK "funds92755eh"
#endif

#define IOT_CONFIG_WIFI_SSID STASSID
#define IOT_CONFIG_WIFI_PASSWORD STAPSK

#define IOT_NAME "Blobklhy3" // "Blockly2" //"<hubname>"
#define IOT_CONFIG_DEVICE_ID "BloxDev" //  "Blockly2_Dev" // "<devicename>"
#define IOT_CONFIG_DEVICE_KEY "scEAgzvavFG21rMypJzkjQzft3W/YyoiidZMVHbED9M=" // "2zqnxw3HLl7ZiuwaPT+96q6KBoSEbgRgdAIoTJMMHzg=" 
// "<device connection key>"

///////////////////////////////////////////////////////////////

#define IOT_PATH ".azure-devices.net"
#define IOT_CONFIG_HUBNAME "Blobklhy3.azure-devices.net" 
#define IOT_CONFIG_HUB_NAME IOT_NAME  IOT_PATH








#endif