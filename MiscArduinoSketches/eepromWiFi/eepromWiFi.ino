#include <WiFi.h>
#include <EEPROM.h>
#include "eepromWiFi.h"


char eeprom[256] = {0};

String Ssid="";
String Passwd="";
String _Ssid="";
String _Passwd="";
String Hostname="picow2";

void setup() {
  Serial.begin(115200);
  while(!Serial);
  bool write2EEProm = false;
  if(!readKey())
  {
    Serial.print("Key not found (");
    Serial.print(KEY);
    Serial.println(") at start of EEProm"); 
    write2EEProm=true;
  }
  else
  {
    Serial.print("Key found. (");
    Serial.print(KEY);
    Serial.println(") at start of EEProm"); 
    Serial.print("Do you wish to reflash the EEProm with new Wifi data? [Y/y] [N/n]");
    while (Serial.available() == 0) {}
    String res = Serial.readString();
    res.toUpperCase();
    res.trim();
    if (res =="Y")
      write2EEProm=true;
  }

  if(write2EEProm)
  {
    Serial.println("Getting new Wifi data and writing key and data.");
    
    //Get SSID

    Serial.print("Enter SSID: ");
    while (Serial.available() == 0) {}
    Ssid = Serial.readString();
    Ssid.trim();

    //Get Password
    Serial.print("Enter Password: ");
    while (Serial.available() == 0) {}
    Passwd = Serial.readString();
    Passwd.trim();

    //Get Hostname
    Serial.print("Enter Hostname. Default ");
    Serial.print("PicoW2: ");
    while (Serial.available() == 0) {}
    String val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      Hostname=val;

    Serial.println("Checking WiFi Data");

    WiFi.mode(WIFI_STA);
    WiFi.setHostname(Hostname.c_str());
    WiFi.begin(Ssid.c_str(), Passwd.c_str());
 
    while (WiFi.status() != WL_CONNECTED) {
      Serial.print(".");
      delay(100);
    }
    Serial.println();
    Serial.println("Connnected.");
    WiFi.disconnect();

    Serial.println("Writing key to EEProm");
    writeKey();
    String wifiData = Ssid +'-'+ Passwd + "-" + Hostname;
    Serial.print("Writing Wifi Config Data to EEProm: ");
    Serial.println(wifiData);

    writeWifi(wifiData,KEYLENGTH);

  }
  else
  {
    Serial.println("Not writing WiFi data to EEProm");
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
  Serial.print("SSID:");
  Serial.println(_Ssid);
  Serial.print("Password:");
  Serial.println(_Passwd);
  Serial.print("Hostname:");
  Serial.println(Hostname);

  Serial.println("Checking WiFi Data");

  WiFi.mode(WIFI_STA);
  WiFi.setHostname(Hostname.c_str());
  WiFi.begin(_Ssid.c_str(), _Passwd.c_str());

  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(100);
  }
  Serial.println();
  Serial.println("Connnected.");
  Serial.println("Wifi data read OK from EEProm. :)");
  
  //print the local IP address
  IPAddress ip = WiFi.localIP();
  Serial.print("RPi Pico W IP Address: ");
  Serial.println(ip);
  WiFi.disconnect();
 
}

void loop()
{

}


void writeWifi(String Msg,int start)
{
  EEPROM.begin(EEPROM_SIZE);
  char * msg = const_cast<char*>(Msg.c_str());
  if ((strlen(msg) )> EEPROM_SIZE )
  {
    Serial.println("Message is too big");
    return;
  }
  int len = strlen(msg);
  for(int i=0; i < len; i++)
  {
    byte val = (byte)msg[i];
    EEPROM.write(start + i, val);
    delay(100); 
  }
  EEPROM.write(start + len, 0);
  if (EEPROM.commit()) {
    Serial.println("EEPROM successfully committed");
  } else {
    Serial.println("ERROR! EEPROM commit failed");
  }
  EEPROM.end();
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
    delay(100);
  }
  target.trim();
  EEPROM.end();
  return target;
}

// First 4 bytes of EEProm are the key then just read WiFi data.
// Else write key and data.
void writeKey()
{
  String key = KEY;
  EEPROM.begin(EEPROM_SIZE);
  char * msg = const_cast<char*>(key.c_str());
  int len = KEYLENGTH;
  for(int i=0; i < len; i++)
  {
    byte val = (byte)msg[i];
    EEPROM.write(i, val);
    delay(100); 
  }
  EEPROM.write(len, 0);
  if (EEPROM.commit()) {
    Serial.println("EEPROM key write successfully committed");
  } else {
    Serial.println("ERROR! EEPROM key write commit failed");
  }
  EEPROM.end();
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
    delay(100);
  }
  target.trim();
  EEPROM.end();
  if (target==key)
    return true;
  else
    return false;
}
