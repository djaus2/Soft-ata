#ifndef CONNECT2WiFi
#define CONNECT2WiFi

#define DEFAULT_SSID "APQLZM"
#define DEFAULT_PASSWORD "tallinn187isnotinaustralia"
#define DEFAULT_HOSTNAME "picow"
#define DEFAULT_BT_NAME "picow SoftataOTA 00:00:00:00:00:00"

// In test sketc:
#define ENABLE_SERIAL_AT_START true

#define EEPROM_SIZE 256

// Key is used to see if the WiFi data has been written to the EEProm
#define KEY  "1370"
#define KEYLENGTH 4



// Read key (in first 4 bytes ) and check
bool readKey();
// Write key in first 4 bytes
void writeKey();

// WiFi Data is stored after the key as string: SSID-Passsword-Hostname
void writeWiFi(String Msg,int start);
String readWiFi(int start);

//////////////////////////////////////

static const char *ConnectMode_STR[] = {
    "wifi_is_set", "from_eeprom", "is_defined", "wifiset", "serial_prompt", "bt_prompt" 
};

enum ConnectMode: byte {wifi_is_set, from_eeprom, is_defined, wifiset, serial_prompt, bt_prompt };

// WiFi connect with current settings
bool WiFiConnect();

// Read connection details for EEProm
bool ReadWiFiDataFromEEProm();

// Prompt user for WiFi connection details and connect
bool Prompt4WiFiConfigData();

// Software set connection settings and connect
void WiFiSet(String ssid, String pwd, String hostname);

// Orchestrate WiFi Connection
bool WiFiConnectwithOptions(int baud, ConnectMode connectMode, bool bwrite2eeprom);


#endif
