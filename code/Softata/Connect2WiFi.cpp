#include <Arduino.h>
#include <WiFi.h>
#include <EEPROM.h>
#include <SerialBT.h>

#include "softata.h"
#include "serial_macros.h"
#include "Connect2WiFi.h"
#include "menu.h"


// Anonymous (Makes it private to this file.):
namespace{
  //Current Connection details
  String Ssid=DEFAULT_SSID;
  String Passwd=DEFAULT_PASSWORD;
  String DeviceName=DEFAULT_DEVICENAME;
  String Hubname=DEFAULT_HUBNAME;
  String IoTHubDeviceConnectionString=DEFAULT_DEVICECONNECTIONSTRING;
  String Guid=DEFAULT_GUID;

  bool bSERIAL_DEBUG = false;

  char eeprom[EEPROM_SIZE] = {0};
}

namespace FlashStorage{

  // Need to set Tools->IP Bluetooth Stack to IPV4 - Bluetooth

  String GetDeviceName()
  {
    return DeviceName;
  }

  String GetIOT_CONFIG_IOTHUB_FQDN()
  {
    return Hubname +  "." + AZUREDEVICESNET;
  }

  String GetHubname()
  {
    return Hubname;
  }

  String GetDeviceConnectionString()
  {
    Serial_println(IoTHubDeviceConnectionString);
    return IoTHubDeviceConnectionString;
  }

  String GetDeviceGuidString()
  {
    return Guid;
  }

  /////////////////////////////////////////////////////////////////////
  // Third Level: Low level read and write EEProm data as strings
  /////////////////////////////////////////////////////////////////////
  bool writeWiFi(String Msg,int start)
  {
    Serial_println("Writing now.");
    EEPROM.begin(EEPROM_SIZE);
    char * msg = const_cast<char*>(Msg.c_str());
    if ((strlen(msg) )> EEPROM_SIZE )
    {
      Serial_println("Message is too big");
      return false;
    }
    int len = strlen(msg);
    for(int i=0; i < len; i++)
    {
      byte val = (byte)msg[i];
      EEPROM.write(start + i, val);
      delay(100); 
    }
    Serial_println("\0");
    EEPROM.write(start + len, 0);
    if (EEPROM.commit()) {
      Serial_println("EEPROM successfully committed");
    } else {
      Serial_println("ERROR! EEPROM commit failed");
    }
    EEPROM.end();
    return true;
  }

  String readWiFi(int start)
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
  /////////////////////////////////////////////////////////////////////


  /////////////////////////////////////////////////////////////////////
  // Second Level: General purpose read and write of string array data
  /////////////////////////////////////////////////////////////////////
  bool ReadStringArrayFromEEProm(String vals[])
  {
    String datas  = readWiFi(KEYLENGTH);
    
    if(Serial)
    {
      Serial_print("EEProm Data: ");
      Serial_println(datas);
    }
    int length =  datas.length();

    // String should begin and end with SEP_CHAR
    if(datas[0] != SEP_CHAR)
    {
      if(Serial)
        Serial_println("No first -");
      return false;
    }
    if(datas[length-1] != SEP_CHAR)
    {
      if(Serial)
        Serial_println("No last -");
      return false;
    };
    //Nb: There is a leading separator in the string and one after each value.
    int splits[NUM_STORED_VALUES+1];
    splits[0] = 0;
    int numSplits = 1;
    for (int i=1;i<NUM_STORED_VALUES;i++)
    {
      int split = datas.indexOf(SEP_CHAR,splits[i-1] +1 );
      if (split == -1)
        break;
      splits[i] = split;
      splits[i+1]= length-1;
      numSplits++;
    }

    if(numSplits != NUM_STORED_VALUES)
    {
      return false;
    }
    for (int i=0;i<NUM_STORED_VALUES; i++)
    {
      vals[i] = datas.substring(splits[i]+1,splits[i+1]);
      vals[i].trim();
    }
    if(Serial)
      Serial_println("Done EEProm Read of Data.");
    return true;
  }

  bool WriteStringArray2EEProm(String datas[])
  {
    if (!bSERIAL_DEBUG) 
      return false;
    if(Serial)
      Serial_println("Writing key to EEProm");
    writeKey();
    String WiFiData = "";
    WiFiData +=  SEP_CHAR;
    for (int i=0;i<NUM_STORED_VALUES;i++)
    {
      WiFiData += datas[i] + SEP_CHAR;
    }
    if(Serial)
    {
      Serial_print("Writing WiFi Config Data to EEProm: ");
      Serial_println(WiFiData);
    }
    String vals[NUM_STORED_VALUES];
    bool res = writeWiFi(WiFiData,KEYLENGTH);
    if(res)
    {
      res = ReadStringArrayFromEEProm(vals);
    }
    if(!res)
    {
      return false;
    }
    for (int i=0;i<NUM_STORED_VALUES;i++)
    {
      if (vals[i] != datas[i])
        return false;
    }
    if(Serial)
      Serial_println("Done EEProm Write of Data.");
    return true;
  }
  /////////////////////////////////////////////////////////////////////

  /////////////////////////////////////////////////////////////////////
  // Top level: Read/Write as entities
  /////////////////////////////////////////////////////////////////////

  // Read connection details for EEProm
  bool ReadWiFiDataFromEEProm()
  {
    String vals[NUM_STORED_VALUES];
    Serial_println("Reading data from EEProm");
    Serial_println("This may take a while.");
    bool res = ReadStringArrayFromEEProm(vals);
    if(!res)
    {
      Serial_println("ReadWiFiDataFromEEProm False");
      return false;
    }
    Ssid  = vals[sv_SSID];
    Passwd = vals[sv_Password];
#ifdef USINGIOTHUB
    DeviceName = vals[sv_hostname];
    Hubname = vals[sv_hubname];
    IoTHubDeviceConnectionString = vals[sv_deviceConnectionString];
#endif
    Guid = vals[sv_guid];
    return true;
  }

    // Write data to EEProm but require user notification
  bool Write2EEPromwithPrompt()
  {
    if (!bSERIAL_DEBUG) 
      return false;
    
    Serial_println("Writing key to EEProm");
    Serial_println("This may take a while.");
    
    writeKey();
    
    String vals[NUM_STORED_VALUES];
    vals[sv_SSID] = Ssid;
    vals[sv_Password] = Passwd;
#ifdef USINGIOTHUB
    vals[sv_hostname] = DeviceName;
    vals[sv_hubname] = Hubname;
    vals[sv_deviceConnectionString] = IoTHubDeviceConnectionString;
#endif
    vals[sv_guid] = Guid;
    
    bool res = WriteStringArray2EEProm(vals);

    if(!res)
      return false;
    
    // Test EEProm data
    if (WiFi.status() != WL_CONNECTED)
    {
      WiFi.disconnect();
      delay(200);
    }
    res = ReadWiFiDataFromEEProm();
    if(res)
    {
      res = WiFiConnect();
    }
    return res;
  }

  /////////////////////////////////////////////////////////////////////

  /////////////////////////////////////////////////////////////////////
  // Keys: Read and write keys (First 4 bytes)
  /////////////////////////////////////////////////////////////////////
  // First 4 bytes of EEProm are the key.
  // Data string follows in the storage.
  // Used to indicate if data has been written.
  // If format changes need to change the key string in the header.
  ///////////////
  void writeKey()
  {
    String key = KEY;
    Serial_println("Writing key to EEProm");
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
      Serial_println("EEPROM key write successfully committed");
    } else {
      Serial_println("ERROR! EEPROM key write commit failed");
    }
    EEPROM.end();
  }

  bool readKey()
  {
    String key = KEY;
    Serial_println("Reading key from EEProm");
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
  ////////////////////////////////////////////////////////////////////



  // WiFi connect with current settings
  bool WiFiConnect()
  {
      if(bSERIAL_DEBUG)
      {
        Serial_print("SSID:");
        Serial_println(Ssid);
        Serial_print("Password:");
        Serial_println(Passwd);

#ifdef USINGIOTHUB
        Serial_print("DeviceName:");
        Serial_println(DeviceName);
        Serial_print("Hubname:");
        Serial_println(Hubname);
        Serial_print("IOT_CONFIG_IOTHUB_FQDN:");
        Serial_println(GetIOT_CONFIG_IOTHUB_FQDN());
        Serial_print("DeviceConnectionString:");
        Serial_println(IoTHubDeviceConnectionString);
#endif
        Serial_print("Guid:");
        Serial_println(Guid);

        Serial_println();
        Serial_println("\t--- Connecting to WiFi ---");
        Serial_print("\t--- Timeout: ");
        Serial_print(WIFI_CONNECT_TIMEOUT_SEC);
        Serial_println(" sec ---");
      }
      WiFi.mode(WIFI_STA);
      WiFi.setHostname(DeviceName.c_str());
      WiFi.begin(Ssid.c_str(), Passwd.c_str());

      //2Do add timeout here
      int maxCount = (int) ((WIFI_CONNECT_TIMEOUT_SEC*1000)/MENU_DELAY);
      int secCount = (int)(1000/MENU_DELAY);
      while (WiFi.status() != WL_CONNECTED) {
        if(bSERIAL_DEBUG)
        {
          if((maxCount-- % secCount ) == 0){
            Serial_print("|");
          }
          else{
            Serial_print(".");
          }
        }
        if (maxCount < 0)
        { 
          Serial_println();
          Serial_println("\t--- WiFi Connect Timeout! ---");
          return false;
        }
        delay(MENU_DELAY);
      }
      if(bSERIAL_DEBUG)
      {
        Serial_println();
        Serial_println("\t--- Connected. ---");
      }
      
      //print the local IP address
      IPAddress ip = WiFi.localIP();
      if(bSERIAL_DEBUG)
      {
        Serial_print("RPi Pico W IP Address: ");
        Serial_println(ip);
      }
      return true;
  }

  // As per WiFiConnect but use Bluetooth Serial instead of Serial
  bool BtWiFiConnect()
  {
      if(true)
      {
        SerialBT.print("SSID:");
        SerialBT.println(Ssid);
        SerialBT.print("Password:");
        SerialBT.println(Passwd);

#ifdef USINGIOTHUB
        SerialBT.print("DeviceName:");
        SerialBT.println(DeviceName);
        SerialBT.print("Hubname:");
        SerialBT.println(Hubname);
        SerialBT.print("IOT_CONFIG_IOTHUB_FQDN:");
        SerialBT.println(GetIOT_CONFIG_IOTHUB_FQDN());
        SerialBT.print("DeviceConnectionString:");
        SerialBT.println(IoTHubDeviceConnectionString);
#endif

        SerialBT.print("Guid:");
        SerialBT.println(Guid);

        SerialBT.println("Connecting to WiFi");
      }

      WiFi.mode(WIFI_STA);
      WiFi.setHostname(DeviceName.c_str());
      WiFi.begin(Ssid.c_str(), Passwd.c_str());

      //2Do add timeout here
      while (WiFi.status() != WL_CONNECTED) {
        SerialBT.print(".");
        delay(250);
      }
      SerialBT.println();
      SerialBT.println("Connected.");
      
      //print the local IP address
      IPAddress ip = WiFi.localIP();
      SerialBT.print("RPi Pico W IP Address: ");
      SerialBT.println(ip);
      return true;
  }



  // Prompt user for WiFi connection details
  // Assumes BT is connected
  bool BTPrompt4WiFiConfigData()
  {
    //Get SSID     
    SerialBT.print("Enter SSID: ");
    while (SerialBT.available() == 0) {}
    Ssid = SerialBT.readString();
    Ssid.trim();
    Serial_println();

    //Get Password
    SerialBT.print("Enter Password: ");
    while (SerialBT.available() == 0) {}
    Passwd = SerialBT.readString();
    Passwd.trim();
    Serial_println();
    String val="";
#ifdef USINGIOTHUB
    //Get DeviceName
    SerialBT.print("Enter DeviceName. Default ");
    SerialBT.print(DeviceName);
    while (SerialBT.available() == 0) {}
    val = SerialBT.readString();
    val.trim();
    if (val.length()!=0)
      DeviceName=val;
    Serial_println();

     //Get Hubname
    SerialBT.print("Enter IoT Hubname. Default ");
    SerialBT.print(Hubname);
    while (SerialBT.available() == 0) {}
    val = SerialBT.readString();
    val.trim();
    if (val.length()!=0)
      Hubname=val;
    Serial_println();

    //Get Device Connection String
    SerialBT.print("Enter IoT Hub Device Connection String. Default ");
    SerialBT.print(IoTHubDeviceConnectionString);
    while (SerialBT.available() == 0) {}
    val = SerialBT.readString();
    val.trim();
    if (val.length()!=0)
      IoTHubDeviceConnectionString=val; 
    Serial_println();
#endif

    //Get GUID
    SerialBT.print("Enter Guid. Default ");
    SerialBT.print(Guid);
    while (SerialBT.available() == 0) {}
    val = SerialBT.readString();
    val.trim();
    if (val.length()!=0)
      Guid=val;
    Serial_println();

     return true;
  }

  // Prompt user for WiFi connection details
  // Requires to be in Debug mode
  bool Prompt4WiFiConfigData()
  {
    if(!bSERIAL_DEBUG)
      return false;

    //Get SSID 
    if(Ssid.length()==0)
      Ssid = DEFAULT_SSID;
    Serial_print("Enter SSID. Default[");
    Serial_print(Ssid);
    Serial_print("]: ");
    while (Serial.available() == 0) {}
    String val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      Ssid = val;
    Serial_println();

    //Get Password
    if(Passwd.length()==0)
      Passwd = DEFAULT_PASSWORD;  
    Serial_print("Enter Password. Default[");
    Serial_print(Passwd);
    Serial_print("]: ");
    while (Serial.available() == 0) {}
    val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      Passwd = val;
    Serial_println();

    //Get DeviceName
    if(DeviceName.length()==0)
      DeviceName = DEFAULT_DEVICENAME;
    Serial_print("Enter DeviceName. Default[");
    Serial_print(DeviceName);
    Serial_print("]: ");
    while (Serial.available() == 0) {}
    val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      DeviceName=val;
    Serial_println();

    //Get Hubname
#ifdef USINGIOTHUB
    if(Hubname.length()==0)
      Hubname = DEFAULT_HUBNAME;
    Serial_print("Enter IoT Hubname. Default[");
    Serial_print(Hubname);
    Serial_print("]: ");
    while (Serial.available() == 0) {}
    val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      Hubname=val;
    Serial_println();

    
    // Get IoT Hub Device Connection String
    if(IoTHubDeviceConnectionString.length()==0)
      IoTHubDeviceConnectionString = DEFAULT_DEVICECONNECTIONSTRING; 
    Serial_print("Enter IoT Hub Device Connection String. Default[");
    Serial_print(IoTHubDeviceConnectionString);
    Serial_print("]: ");
    while (Serial.available() == 0) {}
    val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      IoTHubDeviceConnectionString=val;
    Serial_println();
 #endif

    // Get GUID
    if(Guid.length()==0)
      Guid = DEFAULT_GUID;
    Serial_print("Enter Guid. Default: ");
    Serial_print(Guid);
    while (Serial.available() == 0) {}
    val = Serial.readString();
    val.trim();
    if (val.length()!=0)
      Guid=val;
    Serial_println();

    Serial_println("==== Got EEPROM data to store ====");
    return true;
  }

  // Software set connection settings
  void WiFiSet(String ssid, String pwd, String hostname,String hubname, String deviceconnectionString, String guid )
  {
    Ssid = ssid;
    Passwd = pwd;
#ifdef USINGIOTHUB
    DeviceName = hostname;
    Hubname = hubname;
    IoTHubDeviceConnectionString = deviceconnectionString;
#endif
    Guid = guid;
  }

  


  // Orchestrate WiFi Connection
  bool WiFiConnectwithOptions(ConnectMode connectMode, bool debug, bool doMenu=true) 
  {
    bSERIAL_DEBUG = debug;

    if(bSERIAL_DEBUG)
    {
      if(!Serial)
      {
        Serial.begin();
      }
      while(!Serial);
      delay(100);
      Serial_print("WiFi ConnectMode: ");
      Serial_println(ConnectMode_STR[connectMode]);
    }

    bool res=false;
    String resStr;
    switch (connectMode)
    {
      case wifi_is_set:
        res= true;
        break;
      case from_eeprom:
        if(!readKey())
        {
          if(!bSERIAL_DEBUG)
          {
            res = false;
          }
          else
          {
            Serial_print("Key not found (");
            Serial_print(KEY);
            Serial_println(") at start of EEProm"); 

            Serial_println("Getting WiFi config data.");
            res = Prompt4WiFiConfigData();
            if(res)
            {
              res = WiFiConnect();
            }
            if(res)
            {
              Serial_print("Do you wish to flash the EEProm with this WiFi data? [Y/y] [N/n]");
              while (Serial.available() == 0) {}
              resStr = Serial.readString();
              resStr.toUpperCase();
              resStr.trim();
              Serial_println();
              if (resStr =="Y")
              {
                res =  Write2EEPromwithPrompt();
                if(res)
                {
                  res = ReadWiFiDataFromEEProm();
                }
              }
            }
          }
        }
        else
        {
          String resStr="N";
          res = true;
          if (bSERIAL_DEBUG)
          {
            if(doMenu)
            {
              Serial_print("Key found. (");
              Serial_print(KEY);
              Serial_println(") at start of EEProm"); 
              Serial_print("Do you wish to reflash the EEProm with new WiFi data? [Y/y] [N/n](Default).");

              Serial_println();
              bool resBool = GetMenuChoiceYN(false);
              if (resBool)
              {
                Serial_println("Reading EEProm");
                Serial_println("This may take a while.");
                res = Prompt4WiFiConfigData();
                if(res)
                {
                  //Check
                  res= WiFiConnect();
                }
                if(res)
                {
                  Serial_println("WiFi check Ok: Writing to EEProm");
                  res =  Write2EEPromwithPrompt();
                }
              }
            }
            else
              res = true;
          }
          if(res)
          {
            res = ReadWiFiDataFromEEProm();
          }
        }
        break;
      case is_defined:
        WiFiSet(DEFAULT_SSID, DEFAULT_PASSWORD, DEFAULT_DEVICENAME,DEFAULT_HUBNAME, DEFAULT_DEVICECONNECTIONSTRING, DEFAULT_GUID);    ;
        res = true;
        break;
      case serial_prompt:
        if(!Serial){
          res = false;
        }
        else{
          res = Prompt4WiFiConfigData();
        }
        break;
      case bt_prompt:
        SerialBT.setName(DEFAULT_BT_NAME);
        SerialBT.begin();
        while (!SerialBT);  
        Serial_println("BT Started.");
        Serial_println("Connect BT Terminal now.");
        Serial_println("Once connected, in BT Terminal press |> to continue.");
        while (!SerialBT.available());
        delay(100);
        String dummy = SerialBT.readString();
        res =  BTPrompt4WiFiConfigData();
        break;
      
    }

    if(res)
    {
      res= WiFiConnect();
    }
    return res;
  }

  /////////////////////////////////////////////////////////////








  



}