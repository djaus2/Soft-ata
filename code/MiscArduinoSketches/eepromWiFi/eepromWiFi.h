#ifndef EEPROMWiFi
#define EEPROMWiFi

#define EEPROM_SIZE 256

#define KEY  "1370"
#define KEYLENGTH 4

bool readKey();
void writeKey();

void writeWifi(String Msg,int start);
String readWifi(int start);

#endif
