#include <WiFi.h>
#include <EEPROM.h>
#include "eepromWiFi.h"
#include "serial_macros.h"


char eeprom[256] = {0};

static String Ssid="";
static String Passwd="";
static String _Ssid="";
static String _Passwd="";
static String Hostname="picow2";

String WiFisetup() {
  if(!readKey())
  {
    Serial_print("Key not found (");
    Serial_print(KEY);
    Serial_println(") at start of EEProm");
    return String(""); 
  }
  else
  {
    Serial_print("Key found. (");
    Serial_print(KEY);
    Serial_println(") at start of EEProm"); 
  }

  String data  = readWifi(KEYLENGTH);
  data.trim();
  int split = data.indexOf('-');
  int split2 = data.indexOf('-',split+1);
  _Ssid = data.substring(0,split);
  _Ssid.trim();
  _Passwd = data.substring(split+1,split2);
  _Passwd.trim();
  Hostname = data.substring(split2+1,data.length());
  Hostname.trim();
  Serial_print("SSID:");
  Serial_println(_Ssid);
  Serial_print("Password:");
  Serial_println(_Passwd);
  Serial_print("Hostname:");
  Serial_println(Hostname);

  Serial_println("Checking WiFi Data");

  WiFi.mode(WIFI_STA);
  WiFi.setHostname(Hostname.c_str());
  WiFi.begin(_Ssid.c_str(), _Passwd.c_str());

  while (WiFi.status() != WL_CONNECTED) {
    Serial_print(".");
    delay(250);
  }
  Serial_println();
  Serial_println("Connnected.");
  Serial_println("Wifi data read OK from EEProm. :)");
  
  //print the local IP address
  IPAddress ip = WiFi.localIP();
  Serial_print("RPi Pico W IP Address: ");
  Serial_println(ip);

  return Hostname;
}

String readWifi(int start)
{
  EEPROM.begin(EEPROM_SIZE);
  int address = start;
  byte value = 0;
  String target ="";
  while (((value = EEPROM.read(address++))!= 0) && (address < EEPROM_SIZE))
  {
    char ch = (char)value;
    target += ch;
    delay(10);
  }
  target.trim();
  EEPROM.end();
  return target;
}

bool readKey()
{
  String key = KEY;
  EEPROM.begin(EEPROM_SIZE);
  int address = 0;
  byte value = 0;
  String target ="";
  for (int i=0;i< KEYLENGTH;i++)
  {
    byte value  = EEPROM.read(i);
    char ch = (char)value;
    target += ch;
    delay(10);
  }
  target.trim();
  EEPROM.end();
  if (target==key)
    return true;
  else
    return false;
}
