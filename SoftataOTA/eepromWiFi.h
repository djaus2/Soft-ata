#ifndef EEPROMWiFi
#define EEPROMWiFi

#define EEPROM_SIZE 256

#define KEY  "1370"
#define KEYLENGTH 4

String WiFisetup();

bool readKey();
String readWifi(int start);

#endif
