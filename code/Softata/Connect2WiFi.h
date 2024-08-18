#ifndef CONNECT2WiFi
#define CONNECT2WiFi

#include "softataOTA.h"
#include "softata.h"
#include "NetworkSettings.h"

#define DEFAULT_SSID STASSID
#define DEFAULT_PASSWORD STAPSK
#define DEFAULT_HOSTNAME IOT_CONFIG_DEVICE_ID
#define DEFAULT_HUBNAME IOT_CONFIG_HUBNAME
#define DEFAULT_BT_NAME DEFAULT_HOSTNAME "SoftataOTA 00:00:00:00:00:00"
#define DEFAULT_DEVICECONNECTIONSTRING IOT_CONFIG_DEVICE_KEY
#define DEFAULT_GUID "00000000-0000-0000-0000-000000000000"
#define EEPROM_SIZE 256
#define AZUREDEVICESNET "azure-devices.net"



// Using the 
#define SEP_CHAR '|'
enum StoredVals: byte {sv_SSID, sv_Password, sv_hubname, sv_hostname , sv_deviceConnectionString, sv_guid};
#define NUM_STORED_VALUES 6
#define MIN_NUM_STORED_VALUES 3

// Key is used to see if the WiFi data has been written to the EEProm
#define KEY  "1370"
#define KEYLENGTH 4

enum ConnectMode: byte {wifi_is_set, from_eeprom, is_defined, wifiset, serial_prompt, bt_prompt };


namespace FlashStorage{

static const char *ConnectMode_STR[] = {
    "wifi_is_set", "from_eeprom", "is_defined", "wifiset", "serial_prompt", "bt_prompt" 
};

String GetIOT_CONFIG_IOTHUB_FQDN();
String GetHubname();
String GetDeviceHostname();
String GetDeviceConnectionString();
String GetDeviceGuidString();

// Read key (in first 4 bytes ) and check
bool readKey();
// Write key in first 4 bytes
void writeKey();

//////////////////////////////////////
// WiFi Data is stored after the key as string: SSID-Passsword-Hostname
bool writeWiFi(String Msg,int start);
String readWiFi(int start);

//////////////////////////////////////

bool ReadStringArrayFromEEProm( String datas[]);
bool WriteStringArray2EEProm(String datas[]);

//////////////////////////////////////

bool ReadWiFiDataFromEEProm();
bool Write2EEPromwithPrompt();

//////////////////////////////////////


// WiFi connect with current settings
bool WiFiConnect();

// Prompt user for WiFi connection details and connect
bool Prompt4WiFiConfigData();

// Software set connection settings and connect
void WiFiSet(String ssid, String pwd, String hostname, String hubname, String deviceconnectionString, String guid );

// Orchestrate WiFi Connection
bool WiFiConnectwithOptions(ConnectMode connectMode, bool debug);
}
#endif
