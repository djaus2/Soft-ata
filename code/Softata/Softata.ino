// RPi Pico BSP placed in the public domain by Earle F. Philhower, III, 2022

#include "Softata.h"
#include <softatadevice.h>
#include "rpiboards.h"

#include <WiFi.h>
#include <LEAmDNS.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>

#include "SoftataOTA.h"
#include "Connect2Wifi.h"
#include "menu.h"

#include <rpiwatchdog.h>
#include <softataDevice_sensor.h>
#include <SoftataDevice_environsensors.h>
#include <SoftataDevice_display.h>
#include <SoftataDevice_actuator.h>
#include <SoftataDevice_deviceinput.h>

#include <float.h>

#include "devicesLists.h"

String Hostname = "picow2";

bool useSerialDebug = false;
bool skipMenus = false;

struct CallbackInfo  BME280CBInfo;
// Used when first connected to change inbuilt LED blink rate.
bool hasConnected = false;


// Use one or other in testing
// Only used when Serial1/2 Setup is called from client.
// Comment out both normally.
//#define SERIAL1LOOPBACK
//#define SERIAL2LOOPBACK

//SoftataDevice_Sensor *softataDevice_Sensor;

const char* ssid = STASSID;
const char* password = STAPSK;

#define BUFFSIZE 128
byte Buffer[BUFFSIZE];

int port = PORT;

WiFiServer server(port);

bool first = true;

// Haven't got this to work yet, so use alternative.
// #define USE_TICKER

#ifdef USE_TICKER

#include <Ticker.h>

bool ledState = false;
void blinkTicker() {
  digitalWrite(LED_BUILTIN, ledState);
  ledState = !ledState;
}

//https://github.com/sstaub/Ticker
Ticker Booting(blinkTicker, BOOTING_PERIOD, BOOTING_NUMFLASHES,MILLIS);
Ticker WifiStarted(blinkTicker, WIFI_STARTED_PERIOD, WIFI_STARTED_NUMFLASHES,MILLIS);
Ticker OTAOnStart(blinkTicker, OTA_ON_START_PERIOD, OTA_ON_START_NUMFLASHES,MILLIS);
Ticker OTAOnEnd(blinkTicker, OTA_ON_END_PERIOD, OTA_ON_END_NUMFLASHES,MILLIS);
Ticker OTAOnError(blinkTicker, OTA_ON_ERROR_PERIOD, OTA_ON_ERROR_NUMFLASHES,MILLIS);
Ticker OTAReady(blinkTicker, READY_PERIOD, READY_NUMFLASHES,MILLIS);
void flash(int id, int num, int period)
{
  switch(id)
  {
    case 0:
      Booting.update();
      break;
    case 1:
      WifiStarted.update();
      break;
    case 2:
      OTAOnStart.update();
      break;
    case 3:
      OTAOnEnd.update();
      break;
    case 4:
      OTAOnError.update();
      break;
    case 5:
      OTAReady.update();
      break;
  }
}

void TickerInit()
{
  pinMode(LED_BUILTIN, OUTPUT);
  Booting.start();
  WifiStarted.start();
  OTAOnStart.start();
  OTAOnEnd.start();
  OTAOnError.start();
  OTAReady.start();
}
#else
void flash(int id, int num, int period)
{
  for (int i =0;i<num;i++)
  {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(period/2);
    digitalWrite(LED_BUILTIN, LOW);
    delay(period/2);
  }
}
#endif


bool OTAing=false;

ConnectMode connectMode = WIFI_CONNECT_MODE;

void ArduinoOTAsetup() {
  #ifdef USE_TICKER
  TickerInit();
  #endif

  OTAing =false;

  Serial_println("Booting");
  
  /* Use BuiltIn LED for flashes */
  pinMode(LED_BUILTIN, OUTPUT);

  // Booting: 5 short pulses
  flash(0, BOOTING_NUMFLASHES, BOOTING_PERIOD);



  //From: Connect2WiFi.h:
  //enum ConnectMode: byte {wifi_is_set, from_eeprom, is_defined, wifiset, serial_prompt, bt_prompt };

  
  if((useSerialDebug) &&(!skipMenus))
  {
    Serial_println("WIFI");
    Serial.println("Please select source of Wifi Configuration.");
    Serial.print("Default: "); Serial.print(1+(int)WIFI_CONNECT_MODE); Serial.println(".");
    Serial.println("1. WiFi is Set. Call with parameters.Ignore");
        Serial.println("2. From EEProm.");
            Serial.println("3. Is defined in header.");
                Serial.println("4. Serial prompt for details.");
                    Serial.println("5. Bluetooth prompt for details.");              
   
    int selection = GetMenuChoiceNum(2);
    Serial.println();
    if ((0< selection) && ( 6> selection))
    {
      selection--;
      if ((selection >= wifi_is_set) && (selection <= bt_prompt ))
      {
        connectMode = (ConnectMode)(selection);
      }
    }
  }


  bool Connect2WiFiConnected  = FlashStorage::WiFiConnectwithOptions(connectMode, useSerialDebug, !skipMenus);

  if (Connect2WiFiConnected)
  {
    Serial_println("Used Connect2WIfi for Wifi data.");
  }
  else
  {
    // Fallback
    Serial_println("WiFi connect failed.");
    while(true)
    {
      // If failed just loop here.
      Serial.print("@");
      flash(0xff, 5, 200);
      delay(1000);
    }
  }
  Hostname = FlashStorage::GetDeviceName();
  

  // Got WiFi: 3 long pulses
  flash(1, WIFI_STARTED_NUMFLASHES, WIFI_STARTED_PERIOD);
  
  /* configure  OTA server events */  
  /*******************************************************************/
  ArduinoOTA.setPort(2040);
  // Hostname defaults to pico-[ChipID]
  //ArduinoOTA.setHostname(host);

  // No authentication by default
  // ArduinoOTA.setPassword("admin");

  // Password can be set with it's md5 value as well
  // MD5(admin) = 21232f297a57a5a743894a0e4a801fc3
  // ArduinoOTA.setPasswordHash("21232f297a57a5a743894a0e4a801fc3");

  ArduinoOTA.onStart([]() {  // switch off all hardware
    // Start OTA: 4 extra short pulses. Error if this is too long
    OTAing=true;
    flash(2, OTA_ON_START_NUMFLASHES, OTA_ON_START_PERIOD);
    // Cause an error by making this too long:
    // flash(10, LONGPULSE);
  });


  ArduinoOTA.onProgress([](size_t progresso, size_t total){
    //THandlerFunction_Progress fn
    #ifdef ENABLE_WATCHDOG
      watchdog_update();
    #endif
  });

  ArduinoOTA.onEnd([]() {  // do a fancy thing with our board led at end
    // Done OTA:4 long pulses
    Serial_println("OTA OnEnd");
    flash(3, OTA_ON_END_NUMFLASHES, OTA_ON_END_PERIOD);
  });

  ArduinoOTA.onError([](ota_error_t error) {
    Serial_printf("Error[%u]: ", error);
    // Error: 6 L-O-N-G pulses
    flash(4, OTA_ON_ERROR_NUMFLASHES, OTA_ON_ERROR_PERIOD);
    if (error == OTA_AUTH_ERROR) {
      Serial_println("Auth Failed");
      flash(4, 2, SHORTPULSE);
    } else if (error == OTA_BEGIN_ERROR) {
      Serial_println("Begin Failed");
      flash(4, 4, SHORTPULSE);
    } else if (error == OTA_CONNECT_ERROR) {
      Serial_println("Connect Failed");
      flash(4, 6, SHORTPULSE);
    } else if (error == OTA_RECEIVE_ERROR) {
      Serial_println("Receive Failed");
      flash(4, 8, SHORTPULSE);
    } else if (error == OTA_END_ERROR) {
      Serial_println("End Failed");
      flash(4, 10, SHORTPULSE);
    }

    rp2040.restart();
  });
  /*******************************************************************/
  Serial_println("Nearly ready");
  /* setup the OTA server */
  ArduinoOTA.begin();
  Serial_println("Ready");
  // Ready: 8 ultra short pulses
  flash(5, READY_NUMFLASHES, READY_PERIOD);
}

void ArduinoOTAloop() {
  ArduinoOTA.handle();
}

//////////////////////////////////////////////////////////////////////////////

// Used when first connected to change inbuilt LED blink rate.

// Use one or other in testing
// Only used when Serial1/2 Setup is called from client.
// Comment out both normally.
//#define SERIAL1LOOPBACK
//#define SERIAL2LOOPBACK

//SoftataDevice_Sensor *softataDevice_Sensor;


//Ref: https://www.thegeekpub.com/276838/how-to-reset-an-arduino-using-code/
void (*resetFunc)(void) = 0;

bool RecvdCloud2DeviceMsg = false;
uint32_t popVal; 
byte C2DMessage[e_num+1] ={0};
int Check4RecvdCloud2DeviceMsg()
{
  // Slot Cloud2Device messages in here
  if(RecvdCloud2DeviceMsg)
  {
    Serial_println();
    RecvdCloud2DeviceMsg = false;
    if(popVal>0)
    { 
      uint32_t PopVal = popVal;
      Serial_println(PopVal,HEX);
      int length =0;
      //enum bitStuffingIndex: byte {e_cmd,e_pin,e_param,e_other,e_otherDataCount,e_data1,e_data2};
      C2DMessage[1] = PopVal % (bitStuffing[e_cmd]);
      length++;
      PopVal /= bitStuffing[e_cmd];
      C2DMessage[2] = PopVal % (bitStuffing[e_pin]);
      length++;
      PopVal /= bitStuffing[e_pin];
      C2DMessage[3] = PopVal % (bitStuffing[e_param]);
      length++;
      PopVal /= bitStuffing[e_param];
      C2DMessage[4] = PopVal % (bitStuffing[e_other]);
      length++;
      PopVal /= bitStuffing[e_other];
      C2DMessage[5] = PopVal % (bitStuffing[e_otherDataCount]);
      length++;
      PopVal /= bitStuffing[e_otherDataCount];
      if(PopVal>0)
      {
        C2DMessage[6] = PopVal % (bitStuffing[e_data1]);
        length++;
        PopVal /= bitStuffing[e_data1];
        if(PopVal>0)
        {
          C2DMessage[7] = PopVal % (bitStuffing[e_data2]);
          length++;
          PopVal /= bitStuffing[e_data2];
      } }
      C2DMessage[0] = length;
      RecvdCloud2DeviceMsg = false;
      return C2DMessage[0];
    }
  }
  return 0;
}
void setup()
{  
#ifdef SOFTATA_DEBUG_MODE
    Serial.begin();
    while(!Serial);
    Serial.println();
    Serial.println("\t\tS O F T A T A");
    Serial.println();
    Serial.println();
    Serial.println("Use Serial Debug? [Y] [N]");
    Serial.println("Also [S](default)= Yes but SKIP subsequent menus.");

    // Need useSerialDebug to be set before menu is displayed.
    // YN Menu will determine its state.
    useSerialDebug = true;
    skipMenus = false;
    menu3State resb = GetMenuChoiceYNS(trueButSkipMenus, DEFAULT_MENU_TIMEOUT_SEC);
    switch(resb)
    {
      case trueButSkipMenus:
        useSerialDebug = true;
        skipMenus = true;
        break;
      case ttrue:
        useSerialDebug = true;
        skipMenus = false;
        break;
      case tfalse:
        useSerialDebug = false;
        skipMenus = false;
        break;
      
    }
    if (!useSerialDebug)
    {
      Serial.println("Serial Debug off.");
    }
    else
    {
      Serial.println("Serial Debug on.");
    } 
    Serial.end(); 
#endif
  Serial_begin();
  whileNotSerial();

  #ifdef ENABLE_OTA
    ArduinoOTAsetup();
  #endif
  Serial_println("Done OTA");
  hasConnected = false; 
  
  server.begin();
  Serial_print("Connected to wifi. Address:");
  IPAddress ipAddress = WiFi.localIP();
  Serial_print(ipAddress);
  Serial_print(" Port:");
  Serial_println(port); //port*/
  Serial_print("Server Status: ");
  Serial_println(server.status());
  
  Serial.println();
  Serial.println("Use Azure Service via ngrok? [Y] [N](Default)");
  bool resNgrok = GetMenuChoiceYN(false, DEFAULT_MENU_TIMEOUT_SEC);
  if(resNgrok){
    Serial_println("====================================================================================");
    Serial_println("For remote access run the following on your desktop.");
    Serial_println("(Requires ngrok installation):");
    Serial_print("ngrok tcp ");
    Serial_print(ipAddress);
    Serial_print(":");
    Serial_println(port);
    Serial.println("Note the returned index (the first tuple).eg tcp://4.tcp.ngrok.io:15129, note the 4.");
    Serial_println("====================================================================================");
  }
  // WiFi is started so 2nd core can start
  uint32_t syncVal = initialSynch;
  rp2040.fifo.push(syncVal);
  uint32_t sync = rp2040.fifo.pop();
  

  InitSensorList();
  InitDisplayList();
  InitActuatorList();  
  InitDeviceInputList();
  
  bool first = false;
  // Just to be safe don't simultaneously setup server and client
  uint32_t val = (uint32_t)connectMode;
 
  Serial_print("Initial Core1-Core2 Synch value:");
  Serial_println(val);
  rp2040.fifo.push(val);

  uint32_t sync2 = rp2040.fifo.pop();
  if(val==sync2)
  {
    Serial_println("\t\t==== Core1-Core2 Setup Sync OK ====");
  }
  else
  {
    Serial_println("\t\t==== Core1-Core2 Setup Sync Fail ====");
  }

  flash(10, 8, ULTRA_SHORTPULSE);
  #ifdef ENABLE_WATCHDOG
    watchdog_enable(WATCHDOG_SECS * 1000, false);
  #endif
}

bool Sessioned=false;

void loop() {
  bool KeepAlive = false;
  bool firstLoop = true;
  #ifdef ENABLE_OTA
    ArduinoOTAloop();
  #endif
  #ifdef ENABLE_WATCHDOG
    watchdog_update();
  #endif
  if(OTAing)
      return;
  static int i;
  int count = 0;
  byte msg[maxRecvdMsgBytes];
                              
  delay(500);

  WiFiClient client = server.accept();//server.available();//
  if (!client) {
    return;
  }
  Serial_println("Client connected to Service OK.");

  ////////////////////
  while (KeepAlive || firstLoop || !Sessioned) {
    firstLoop = false;  // Only do this once unless KeepAlive is set
    #ifdef ENABLE_OTA
      ArduinoOTAloop();
    #endif
    #ifdef ENABLE_WATCHDOG
      watchdog_update();
    #endif
    if(OTAing)
        return;
    Serial_print("Get next command.");
    while (!client.available()) {
      delay(100);
      #ifdef ENABLE_OTA
        ArduinoOTAloop();
      #endif
      #ifdef ENABLE_WATCHDOG
        watchdog_update();
      #endif
      if(OTAing)
          return;
      if(rp2040.fifo.available())
        break;
    }
    if(rp2040.fifo.available())
    {
      popVal = rp2040.fifo.pop();
      RecvdCloud2DeviceMsg = true;
    }
    byte length=0;
    count = 0;
    if(RecvdCloud2DeviceMsg)
    {
      count=Check4RecvdCloud2DeviceMsg();
    }
    if (count>0)
    {
      length = C2DMessage[0];
      Serial_print("C2DMsg:");
      Serial_print("length=0x");
      Serial_print(C2DMessage[0],HEX);
      Serial_print(' ');
      Serial_print("cmd=0x");
      Serial_print(C2DMessage[1],HEX);
      Serial_print(' ');
      Serial_print("pin=0x");
      Serial_print(C2DMessage[2],HEX);
      Serial_print(' ');
      Serial_print("param=0x");
      Serial_print(C2DMessage[3],HEX);
      Serial_print(' ');
      Serial_print("other=0x");
      Serial_print(C2DMessage[4],HEX);
      if(length>5)
      {
        Serial_print(" [");
        Serial_print(C2DMessage[5],HEX);  
        if(true)
        {
          Serial_print(' ');
          Serial_print(C2DMessage[6],HEX);      
          if(length>7)
          {
            Serial_print(' ');
            Serial_print(C2DMessage[7],HEX);      
            if(length>8)
            {
              Serial_print(' ');
              Serial_print(C2DMessage[8],HEX);
            }
          }
        }
        Serial_print("]");
      }
      Serial_println();
      for(int i=0; i<length; i++)
      {
        msg[i] = C2DMessage[i+1];
      }
      //continue;
    }
    else
    {
      count = 0;
      length = 0;
      Serial_println("...Is connected.");
      if(!hasConnected)
      {
        hasConnected = true;
        rp2040.fifo.push( SynchMultiplier + (int)svrConnected); //Make Inbuilt LED flash faster
        while (!rp2040.fifo.available())
        {
          #ifdef ENABLE_WATCHDOG
            watchdog_update();
          #endif
        }
        uint32_t sync = rp2040.fifo.pop();
      }
      length = client.read();
      Serial_print(length);
      Serial_print('-');
      count = 0;
      //byte msg[maxRecvdMsgBytes];
      if (length == 0) {
        Serial_println("Null msg.");
        return;
      }
      while (client.available() && (count != length)) {
        msg[count] = client.read();
        if(msg[0]<0xD0) 
        {
          if(count==0)
          {
            Serial_print((char)msg[count]);
            Serial_print(' ');
          }
        }
        else 
        {
          Serial_print(msg[count], HEX);
          //if(count<2)
            Serial_print(' ');
        }
        watchdog_update();
        count++;
      }
    }
    Serial_println();
    if (count != length) {
      Serial_print("count:");
      Serial_println(count);
      Serial_print("length:");
      Serial_println(length);
      Serial_println("Msg invalid.");
      return;
    }
    switch (msg[0]) {
      // Escape simple string commands here.
      // These commands to start in uppercase
      // Need first letter of these ASCII values to not match commands
      // ie Softata commands not to be between 65 to 90, 0x41 to 0x5A
      // 'A' to 'Z'
      case (byte)'K':  //Set KeepAlive with sequence of commands from one APS.NET controller method, ie same session connection
        KeepAlive = true;  //This is cleared with 'A'  Used with Blockly
        Sessioned = true;  // This defaults to false.  Need to set first time 'K' is called so that disconnection occurs after commands
        Serial.println("KeepAlive");
        client.write("KeepAlive");
        break;
        /// If a Controller command actually calls a number of other Controller methods, preface with 'K and end with 'A'
      case (byte)'A': // (Default) Disconnect after each command, which requires reconnection for each.
        KeepAlive = false;  //See above
        Serial.println("Ack");
        client.write("Ack");
        break;
      case (byte)'B':  // Begin
        Serial_println("Ready.");
        client.print("Ready");  // Sent at first connection.
        break;
      case (byte)'E':  //End
        Serial_println("Done.");
        client.print("Done");
        // Force reset
        resetFunc();
        break;
      case (byte)'N':  //Null
        Serial_println("Null.");
        client.print("OK");
        return;
        break;
      case (byte)'R':  //Reset
        Serial_println("Resetting.");
        client.print("Reset");
        // Force reset
        resetFunc();
        break;
       case (byte)'V':  //Get Version
        Serial_println(APP_VERSION);
        client.print(APP_VERSION);
        break;
       case (byte)'D':  //Get Device Types
       {
        String devicesCSV = SoftataDevice::GetListofDevices();
        Serial_println(devicesCSV);
        client.print(devicesCSV);
       }
        break;
      case (byte)'S':  //Get base for sensor/display/actuator commands=0xf0
       {
        byte base = SoftataDevice::GetSoftataDeviceCmdBase();
        Serial_println(base);
        client.print(base);
       }
       break;
      case (byte)'T':  // Using session at client end (ie Blockly), ie KeepAlived used
       {
        Sessioned = true;
        Serial_println("Sessioned");
        client.print("Sessioned");
       }
        break;
      case (byte)'U':  // Not using sessioned, ie KeepAlived not used
       {
        Sessioned = false;
        Serial_println("UnSessioned");
        client.print("UnSessioned");
       }
        break;
       case (byte)'O':
       case (byte)'W':  //Cause WatchDog or OTA failure with busy wait with no WDT or OTA updates.
      {
        if(!Serial)
        {
          Serial.begin(115200);
          while(!Serial)
          {
            delay(250);
          }
        }
        Serial.println();
        Serial.println("Generating WDT and/or OTA failure.");
        Serial.printf("If WDT test, shouild get about: [%u] USB serial messages before reboot: ", WATCHDOG_SECS );
        Serial.println();
        for (int i=1;i<100;i++)
        {
          Serial.printf("WDT count: [%u]sec: ", i);
          Serial.println();
          delay(1000);
        } 
       }  
        break;
       default:
        // Get Softata command and parameters
        byte cmd = msg[0];
        byte pin = 0xff;
        byte param = 0xff;
        byte other = 0xff;
        byte * otherData = NULL;
        if (length > 1) {
          pin = msg[1];
          if (length > 2) {
            param = msg[2];
            if (length > 3) {
              other = msg[3];
              if(length>4)
              {
                otherData = msg+4;
              }
            }
          }
        }
      
        if(cmd == SOFTATADEVICE_ACTUATOR_CMD)
        {
          if(other>=LED)
          {
            Serial_println("Pivoting Actuator to Relay ");
            other=RELAY;
          }
        }

        Serial.println(other);
        Serial.println("=================");

        //Print command and paramaters
        String str;
        byte vaue;
        Serial_print("cmd:");
        Serial_print(cmd, HEX);
        Serial_print(' ');
        if (pin != 0xff) {
          Serial_print("pin:");
          Serial_print(pin, HEX);
          Serial_print(' ');
          if (param != 0xff) {
            Serial_print("param:");
            Serial_print(param,HEX);
            Serial_print(' ');
            if (other != 0xff) {
              Serial_print("other:");
              Serial_print(other,HEX);
            }
          }
        }
        Serial_print(' ');
        if(otherData!= NULL)
        {
          Serial_print(" otherData:[");
          for(int i=0; i<=otherData[0];i++)
          {
            Serial_print(otherData[i],HEX);
            if(i<otherData[0])
            {
              Serial_print(',');
            }
            else
            {
              Serial_print(']');
            }
          }
        }
        Serial_println();
        // Action cmds
        int value;
        switch (cmd) {
          case 0xD0:
          case 0xD1:
          case 0xD2:
          case 0xD3:
            {
              // Digital
              if (!IS_PIN_DIGITAL(pin)) {
                Serial_print("Pin is not digital");
                client.print("FAIL");
                continue;
              }
              switch (cmd) {
                case 0xD0:
                  Serial_print("pinMode:");
                  Serial_println(param);
                  pinMode(pin, (PinMode)param);
                  client.print("OK:");
                  break;
                case 0xD2:
                  Serial_println("digitalRead");
                  value = digitalRead(pin);
                  if (value)
                    client.print("ON");
                  else
                    client.print("OFF");
                  break;
                case 0xD1:
                  Serial_print("digitalWrite:");
                  Serial_println(param);
                  if (param)
                    digitalWrite(pin, HIGH);
                  else
                    digitalWrite(pin, LOW);
                  client.print("OK");
                  break;
                case 0xD3:
                  Serial_println("digitalToggle");
                  value = digitalRead(pin);
                  if (value)
                    digitalWrite(pin, LOW);
                  else
                    digitalWrite(pin, HIGH);
                  client.print("OK");
                  break;
                default:
                  Serial_println("Unknown digital cmd");
                  client.print("Unknown digital cmd");
                  break;
              }
              break;
            }
          case 0xA2:  // Analog place holder
            if (!IS_PIN_ANALOG(pin)) {
              Serial_print("NOK: Pin not Analog");
              client.print("FAIL");
              continue;
            } else {
              if (cmd == 0xA2) {              
                value = analogRead(pin);
                String msgAD = "AD:";
                msgAD += value;
                Serial_println(msgAD);
                client.print(msgAD);
                //break;
              }
            }
            break;
          case 0xA3:  // 10 or 12 bit ADC
            if (cmd == 0xA3) { 
              byte resolution = param;
              String msg = "";
              if(resolution == 0xff)
              {
                msg = "NOK:No resolution supplied.";
                Serial.println(msg);
                client.print(msg);
                continue;
              } else if (resolution == 10) {
                msg = "10 bit/1023 Max";
                analogReadResolution(param);
              } else if (resolution == 12) {
                msg = "12 bit/4095 Max";
                analogReadResolution(param);
              } 
              else {
                msg = "NOK:Unknown resolution: ";
                msg += resolution;
                Serial.println(msg);
                client.print(msg);
                continue;
              }          
              String msgAD = "AD:";
              msgAD += msg;
              Serial_println(msgAD);
              client.print(msgAD);
              //break;
            }
            break;
                   
          
          case 0xB0:  // 4 to 16 bit PWM
            if (cmd == 0xB0) { 
              String msgAD = "";
              byte resolutionBits = param;
              String msg = "";
              if(resolutionBits == 0xff)
              {
                  msgAD = "NOK: SetPWM Resolution. Not supplied.";
                  Serial.println(msgAD);
                  client.print(msgAD);
                  continue;
              }             
              if(resolutionBits<4)
              {
                  msgAD = "NOK: SetPWM Resolution Bits too small: ";
                  msgAD += resolutionBits;
                  msgAD += " Needs to be 4 to 16.";
                  Serial.println(msgAD);
                  client.print(msgAD);
                  continue;
              }
              else if(resolutionBits>16)
              {
                  msgAD = "NOK: SetPWM Resolution Bits too large: ";
                  msgAD += resolutionBits;
                  msgAD += " Needs to be 4 to 16.";
                  Serial.println(msgAD);
                  client.print(msgAD);
                  continue;
              }
              analogWriteResolution(resolutionBits);
              msgAD ="PW: SetPWM Resolution Bits: ";
              msgAD += resolutionBits;
              Serial_println(msgAD);
              client.print(msgAD);
            }
            break;   
          case 0xB1:  // PWM
            if (!IS_PIN_PWM(pin)) {
              Serial_print("Pin not PWM");
              client.print("FAIL");
              continue;
            }
            if (cmd == 0xB1) {
              Serial_print("PWM");
              int value = otherData[1]+otherData[2]*256;
              analogWrite(pin, value);
              Serial_print("PWM:analogWrite(");
              Serial.print(value);
              Serial.println(")");
              client.print("PW: AnalogWrite()");
            }
            break;
          case 0xC0:
          case 0xC1:
          case 0xC2:  // Servo place holder
            if (!IS_PIN_SERVO(pin)) {
              Serial_print("Pin not Servo");
              client.print("FAIL");
              continue;
            }
            Serial_println("OK-SERVO 2D cmds");
            client.print("OK-SERVO 2D cmds");
            break;
          case 0xE0:  // Setup Serial1/2
          case 0xE1:  // Get a char
          //case 0xE2: // Get a string
          //case 0xE3: // Get a string until char
          case 0xE4:  // Write a char
                      //case 0xE5: // Get Flost
                      //case 0xE6: // Get Int
          case 0xE7:
            if (!((other == 1) || (other == 2))) {
              //Nb: For serial except for setup, pin parameter is ignored
              // But need to specify Serial1 or Serial2 as 1 or 2 in other parameter
              Serial_print("Not Serial 1 or 2 (other)");
              client.print("FAIL");
              continue;
            } else {
              Stream& Comport = Serial1;  //Comport doesn't work for some aspects of Serial2
              if (other == 2) {
                Serial_println("Serial2");
                Comport = Serial2;
              } else {
                Serial_println("Serial1");
              }

              char ch;
              String str;
              String msgSerial;
              float fNum;
              int iNum;
              switch (cmd) {
                case 0xE0:  // Set Pins (Provide Tx, Determine Rx) and set Baudrate from list
                  {

#ifdef GROVE_RPI_PICO_SHIELD
                  byte Tx = UART0TX;
                  byte Rx = UART0RX;
                  if(other==2) {
                    Tx = UART1TX;
                    Rx = UART1RX;
                  }
#elif defined(RPI_PICO_DEFAULT)
                    byte Tx = pin;
                    byte Rx = pin + 1;
#endif
                    if (IS_PIN_SERIAL_TX(Tx)) {
                      int baudrate = Baudrates[param];
                      if (other == 1) {
                        Serial1.setTX(Tx);
                        Serial1.setRX(Rx);
                        //Serial1.SetRTS()
                        //Serial1.setCTS();
                        Serial1.begin(baudrate, SERIAL_8N1);
#ifdef SERIAL1LOOPBACK
                        Serial_println("Serial1 Loopback");
                        Serial_print("Tx:");
                        Serial_print(Tx);
                        Serial_print("  Rx:");
                        Serial_println(Rx);
                        Serial_println("Serial1 Loopback Test Sending 64");
                        delay(500);
                        Serial2.write(64);
                        while (!Serial1.available()) { delay(100); };
                        byte chw = Serial1.read();
                        Serial_print("Serial1 Loopback Test Got:0x");
                        Serial_println(chw, HEX);
#endif
                        Serial_println("Serial1.setup");
                        client.print("OK:");
                      } else if (other == 2) {
                        Serial2.setTX(Tx);
                        Serial2.setRX(Rx);
                        Serial2.begin(baudrate, SERIAL_8N1);
#ifdef SERIAL2LOOPBACK
                        Serial_println("Serial2 Loopback");
                        Serial_print("Tx:");
                        Serial_print(Tx);
                        Serial_print("  Rx:");
                        Serial_println(Rx);
                        Serial2.begin(baudrate, SERIAL_8N1);
                        Serial_println("Serial2 Loopback Test Sending 64");
                        delay(500);
                        Serial2.write(64);
                        while (!Serial2.available()) { delay(100); };
                        byte chw = Serial2.read();
                        Serial_print("Serial2 Loopback Test Got:0x");
                        Serial_println(chw, HEX);
#endif
                        Serial_println("Serial2.setup");
                        client.print("OK:");
                      }
                    } else {
                      Serial_println("Serial.setup Fail");
                      client.print("Fail");
                    }
                  }
                  break;
                case 0xE1:
                  if(other==1)
                  {
                    while (!Serial1.available()) { delay(100); }
                    ch = Serial1.read();
                    Serial_print("Serial2.readChar:");
                  }
                  else if(other==2)
                  {
                    while (!Serial2.available()) { delay(100); }
                    ch = Serial2.read();
                    Serial_print("Serial2.readChar:");                                       
                  }
                  msgSerial = "OK:";
                  msgSerial.concat((byte)ch);
                  client.print(msgSerial);
                  break;
                  /*               case 0xE2:
                  // Read String Until Timeout
                  while (Comport.available() == 0) {delay(100);} 
                  str = Serial.readString();
                  Serial_print("Serialn.readStringUntilTimeout:");
                  Serial_println(str);
                  msgSerial = "SER:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break;
                case 0xE3:
                  // Read string until
                  while (Serial1.available() == 0) {delay(100);} 
                  str = Comport.readStringUntil((char)param);
                  Serial_print("Serialn.readStringUntil:");
                  Serial_println(str);
                  msgSerial = "SER:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break;   */
                case 0xE4:
                  // Write char
                  ch = (char)param;
                  if (other == 1)
                  {
                    Serial1.write(ch);
                    Serial_println("Serial1.writeChar");
                  }
                  else if(other==2)
                  {
                    Serial2.write(ch);
                    Serial_println("Serial2.writeChar");                   
                  }
                  client.print("OK:");
                  break;
                  /*               case 0xE5:
                  while(!Comport.available()){ delay(100);}
                  fNum  = Comport.parseFloat();
                  Serial_print("Serialn.readFloat:");
                  Serial_println(fNum);
                  str = String(fNum);
                  msgSerial = "FLT:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break; 
                case 0xE6:
                  while(!Comport.available()){ delay(100);}
                  iNum  = Comport.parseInt();
                  Serial_print("Serialn.readInt:");
                  Serial_println(iNum);
                  str = String(iNum);
                  msgSerial = "INT:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break;    */
                case 0xE7:
                  {
                    String msgSerial = "OK:";
                    String line="";
                    if (other == 1)
                    {
                      while(!Serial1.available()){ watchdog_update();delay(100);}
                      line = Serial1.readStringUntil('\n');
                    }
                    else if(other==2)
                    {
                      while(!Serial2.available()){ watchdog_update();delay(100);}
                      line = Serial2.readStringUntil('\n');               
                    }
                    else
                    {
                      client.print("Fail:Serial.Readlin()");
                      break;
                    }
                    msgSerial.concat(line);
                    Serial_println(msgSerial);
                    client.print(msgSerial);
                  }
                  break;
              }
            }
            break;
          case SOFTATADEVICE_SENSOR_CMD: //Grove Sensors
            { 
              // Cmd = 0xF0,pin=Index of cmd in list,param=SubCmd,Other=
              // GroveSensorCmds{
              // s_getPinsCMD, s_getPropertiesCMD, 
              // s_setupDefaultCMD, s_setupCMD, s_readallCMD, s_readCMD, s_getSensorsCMD=255 
              //};
              SoftataSensor _sensor = (SoftataSensor)other;
              Serial.print("Sensor/LLN: ");Serial.println(_sensor);
              Serial.print("Sensor CMD: "); Serial.println(param);
              switch (param)
              {
                case S_getPinsCMD: 
                  {        
                    String pins =  "OK:";
                    pins.concat(SoftataDevice_Sensor::GetPinout (_sensor));
                    client.print(pins); 
                  }
                  break;
                case s__getPropertiesCMD:
                  {
                    String props ="OK:";

                    props.concat(SoftataDevice_Sensor::_GetListofProperties(_sensor));
                    Serial.println(props);
                    client.print(props);
                  }
                  break;
                case S_setupDefaultCMD:
                case S_setupCMD:
                  SoftataDevice_Sensor * softatadevice_Sensor;
                  if(param==S_setupDefaultCMD)
                  {
                    softatadevice_Sensor = SoftataDevice_Sensor::_Setup(_sensor);
                    if(softatadevice_Sensor != NULL)
                    {
                      Serial_println("Default Sensor Setup");
                      int index = AddSensorToList(softatadevice_Sensor);
                      Serial_print("Sensor Index: ");
                      Serial.println(index);
                      String msgSettingsS1 = "OK";
                      msgSettingsS1.concat(':');
                      msgSettingsS1.concat(index);
                      client.print(msgSettingsS1);
                    }
                    else
                      client.print("Fail:Sensor.Setup");
                    }
                  else
                  {
                    byte bsettings[1];
                    Serial_print("Non-Default Sensor Setup Pin: ");
                    Serial_print(pin);
                    bsettings[0] = pin;
                    softatadevice_Sensor = SoftataDevice_Sensor::_Setup(_sensor,bsettings,1);
                    if(softatadevice_Sensor != NULL)
                    {
                      int index = AddSensorToList(softatadevice_Sensor);
                      Serial_print("Sensor Index: ");
                      Serial_println(index);
                      String msgSettingsS2 = "OK";
                      msgSettingsS2.concat(':');
                      msgSettingsS2.concat(index);
                      client.print(msgSettingsS2);
                    }
                    else
                      client.print("Fail:Sensor.Setup");
                  }
                  break;
                case s_readallCMD:
                  {
                                                                                    Serial.println(_sensor);
                                                                                    Serial.println(other);
                    SoftataDevice_Sensor * softatadevice_Sensor = GetSensorFromList(other);
                    //_sensor = SoftataDevice_Sensor::GetIndexOf(softatadevice_Sensor->num_properties);
                    int num = softatadevice_Sensor->num_properties;
                                                                                    Serial.println(num);
                    double values[num];
                    Tristate ts = softatadevice_Sensor->ReadAll(values);
                    if (ts==notImplemented)
                    {
                      client.print("OK:Sensor.readall Not Implemented");
                    }
                    else if((ts==_nok) || (ts==_nan))
                    {
                      client.print("Fail:Sensor.readall");
                    }
                    else
                    {
                      String msgGetAll = "OK:";
                      for(int i=0;i<num;i++)
                      {
                        msgGetAll.concat(values[i]);
                        if(i<(num-1))
                          msgGetAll.concat(",");
                      }
                      Serial.println(msgGetAll);
                      client.print(msgGetAll);
                    }
                  }
                  break;
                case s_readoneCMD:
                  {
                    SoftataDevice_Sensor * softatadevice_Sensor = GetSensorFromList(other);
                    // A bit of reuse of real-estate here:
                    byte property = pin;
                    if(property>(softatadevice_Sensor->num_properties-1))
                    {
                      client.print("Fail:Read Property no. > no. properties");
                    }
                    else
                    {
                      double value = softatadevice_Sensor->Read(property);
                      if (value != DBL_MAX)
                      {
                        String msgGetOne = "OK:";
                        msgGetOne.concat(value);
                        client.print(msgGetOne);
                      }
                      else
                      {
                        client.print("Fail:Read");
                      }
                    }
                  }
                  break;
                case s_getTelemetry:
                  {
                    int index = other;
                    SoftataDevice_Sensor * softatadevice_Sensor = GetSensorFromList(index);
                    String json = softatadevice_Sensor->GetTelemetry();
                    String ret = "OK:";
                    ret.concat(json);
                    client.print(ret);
                  }
                  break; 
                case s_sendTelemetryBT:
                  {
                    //TelemetryStreamNo
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      SoftataDevice_Sensor * softatadevice_Sensor  = node->Sensor;
                      // Note: Each sensor instance has a private CallbackInfo property
                      CallbackInfo * info = softatadevice_Sensor->GetCallbackInfo();
                      if(info==NULL)
                      {
                        Serial_println("IsNull");
                      }
                      else
                      {

                      }

                      unsigned long period=5000l;
                      byte settings[maxNumDisplaySettings]; 
                      int numSettings=0;
                      if (otherData!= NULL)
                      {
                        for (int i=0; ((i< otherData[0]) &&(i<=maxNumDisplaySettings));i++)
                        {
                          settings[i] = otherData[i+1];
                          numSettings++;
                        }
#ifdef TELEMETRY_DOUBLE_FLASH_INBUILT_LED
                        // Double flash takes about 1 sec of processing, in second core
                        if (settings[0]<2)
                          settings[0] = 2;
#endif 
                        period = settings[0] * 1000l; // Milliseconds
                      }

                      Serial_print("period BT");
                      Serial_println(period);

                      info->period=period;
                      info->isSensor=true;
                      info->sendBT=true;
                      info->SensorIndex = index;
                      int SensorListIndex = AddCallBack(info);
                      node->TelemetryStreamNo = SensorListIndex;
                      msg = String("OK:");
                      msg.concat(SensorListIndex);
                    }
                    else
                    {
                      msg=String("Fail:SendTelemetryBT-Sensor not found.");
                    }
                    client.print(msg);
                    Serial_println(msg);
                  }
                  break; 
                case s_sendTelemetryToIoTHub:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);                   
                    String msg;
                    if(node != NULL)
                    {
                      SoftataDevice_Sensor * softatadevice_Sensor  = node->Sensor;
                      // Note: Each sensor instance has a private CallbackInfo property
                      CallbackInfo * info = softatadevice_Sensor->GetCallbackInfo();

                      unsigned long period=5000l;
                      byte settings[maxNumDisplaySettings]; // Allow 4 settings for nw.
                      int numSettings=0;
                      if (otherData!= NULL)
                      {
                        for (int i=0; ((i< otherData[0]) &&(i<=maxNumDisplaySettings));i++)
                        {
                          settings[i] = otherData[i+1];
                          numSettings++;
                        }
#ifdef TELEMETRY_DOUBLE_FLASH_INBUILT_LED
                        // Double flash takes about 1 sec of processing in second core
                        if (settings[0]<2)
                          settings[0] = 2;
#endif                       
                        period = settings[0]  * 1000l; // Milliseconds
                      }

                      info->period=period;
                      info->isSensor=true;
                      info->sendBT=false;
                      info->SensorIndex = index;

                      // Lock around Callbacks
                      rp2040.idleOtherCore();
                      int SensorListIndex = AddCallBack(info);
                      node->TelemetryStreamNo = SensorListIndex;
                      rp2040.resumeOtherCore();

                      msg = String("OK:");
                      msg.concat(SensorListIndex);
                    }
                    else
                    {
                      msg=String("Fail:SendTelemetryToIoTHub-Sensor not found");
                    }
                    Serial_println(msg);
                    client.print(msg);
                  } 
                  break; 
                case s_pause_sendTelemetry:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      // Encapsulate the TelemetryStreamNo and oause (ie 0) in one value
                      int num = node->TelemetryStreamNo*SynchMultiplier + pauseTelemetryorBT;
                      rp2040.fifo.push(num);
                      while (!rp2040.fifo.available())
                      {
                          watchdog_update();
                      }
                      uint32_t sync = rp2040.fifo.pop();
                      if(sync == 1)
                      {
                        msg = String("OK:");
                      }
                      else
                      {
                        msg = String("Fail:Pause_sendTelemetry()-Pause error");
                      }
                    }
                    else
                    {
                      msg = String("Fail:Pause_sendTelemetry()-Sensor not found");
                    }
                    Serial_println(msg);
                    client.print(msg);
                  } 
                  break;  
                case s_continue_sendTelemetry:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      // Encapsulate the TelemetryStreamNo and run/continue (ie 1) in one value
                      int num = node->TelemetryStreamNo*1000 + continueTelemetryorBT;
                      Serial_print("s_continue_sendTelemetry num:");
                      Serial_println(num);
                      rp2040.fifo.push(num);
                      while (!rp2040.fifo.available())
                      {
                          watchdog_update();
                      }
                      uint32_t sync = rp2040.fifo.pop();
                      if(sync == 1)
                      {
                        msg = String("OK:");
                      }
                      else
                      {
                        msg = String("Fail:Pause_sendTelemetry()-Pause error");
                      }
                    }
                    else
                    {
                      msg = String("Fail:Pause_sendTelemetry()-Sensor not found");
                    }
                    Serial_println(msg);
                    client.print(msg);
                  } 
                  break; 
                case s_stop_sendTelemetry:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      // Encapsulate the TelemetryStreamNo and oause (ie 0) in one value
                      int num = node->TelemetryStreamNo*SynchMultiplier + stopTelemetryorBT;
                      Serial_print("Stop Telemetry num:");
                      Serial_println(num);
                      rp2040.fifo.push(num);
                      while (!rp2040.fifo.available())
                      {
                          watchdog_update();
                      }
                      uint32_t sync = rp2040.fifo.pop();
                      if(sync == 1)
                      {
                        msg = String("OK:Stop Telemtry");
                      }
                      else
                      {
                        msg = String("Fail:Stop_sendTelemetry()-Pause error");
                      }
                    }
                    else
                    {
                      msg = String("Fail:Pause_sendTelemetry()-Sensor not found");
                    }
                    Serial_println(msg);
                    client.print(msg);
                  }
                  break;   
                case S_getCmdsCMD:
                  {
                    String scmmds ="OK:";
                    scmmds.concat(SoftataDevice_Sensor::GetListofCmds());
                    client.print(scmmds);
                  }     
                case S_getDevicesCMD:
                  {
                    String msg = String("OK:");
                    msg.concat(SoftataDevice_Sensor::GetListofDevices());
                    Serial_println(msg);
                    client.print(msg);
                  }
                  break;
                default:
                    Serial_println("Sensor Unknown cmd");
                    client.print("OK:Sensor Unknown cmd");
                    break;
              }
            }
            break;
          case SOFTATADEVICE_DISPLAY_CMD: //Grove Displays
            {
              //#define G_DISPLAYS C(OLED096)C(LCD1602)C(NEOPIXEL)
              // enum GroveDisplayCmds{
              // d_getPinsCMD, d_tbdCMD, d_setupDefaultCMD, d_setupCMD, 
              // d_clearCMD,d_backlightCND,d_setCursorCMD,homeCMD,d_miscCMD, d_getDisplaysCMD=255 
              // }
              SoftataDisplay _display = (SoftataDisplay)other;
              SoftataDevice_Display * softataDevice_Display;
              switch (param)
              {
                case D_getCmdsCMD: //get Generic Display Cmds
                {
                  String cmds ="OK:";
                  cmds.concat(SoftataDevice_Display::GetListofCmds());
                  client.print(cmds);
                }
                break;
                case D_getPinsCMD: //getPins
                {
                  String pins =  "OK:";
                  pins.concat(SoftataDevice_Display::GetPinout (_display));
                  client.print(pins);
                }
                break;
                case d__miscGetListCMD: //Get list of Msic commands for device
                {
                  String misccmds ="OK:";
                  misccmds.concat(SoftataDevice_Display::_GetListofMiscCmds(_display));
                  Serial.println(misccmds);
                  client.print(misccmds);        
                }
                break;
                case D_setupDefaultCMD: //Setupdefault
                case D_setupCMD: //Setup(params)
                {
                  if(param==D_setupDefaultCMD)
                  {
                    softataDevice_Display = SoftataDevice_Display::_Setup(_display);
                    if(softataDevice_Display != NULL)
                    {
                      Serial_println("Default Display Setup");
                      int index = AddDisplayToList(softataDevice_Display);
                      Serial_print("Display Index: ");
                      Serial.println(index);
                      String msgSettingsD1 = "OK";
                      msgSettingsD1.concat(':');
                      msgSettingsD1.concat(index);
                      client.print(msgSettingsD1);
                    }
                    else
                      client.print("Fail:Display.Setup");
                  }
                  else
                  {
                    byte bsettings[1];
                    Serial_print("Non-Default Display Setup Pin: ");
                    Serial_print(pin);
                    bsettings[0] = pin;
                    softataDevice_Display = SoftataDevice_Display::_Setup(_display,bsettings,1);
                    if(softataDevice_Display != NULL)
                    {
                      int index = AddDisplayToList(softataDevice_Display);
                      Serial_print("Display Index: ");
                      Serial_println(index);
                      String msgSettingsD2 = "OK";
                      msgSettingsD2.concat(':');
                      msgSettingsD2.concat(index);
                      client.print(msgSettingsD2);
                    }
                    else
                      client.print("Fail:Display.Setup");
                  }
                }
                break;
                case d_dummyCMD:
                {
                  int index = other;
                  softataDevice_Display = GetDisplayFromList(index);
                  Tristate ts = softataDevice_Display->Dummy();
                  if(ts == _ok)
                  {
                    client.print("OK:Dummy");
                  }
                  else if (ts==notImplemented)
                  {
                    client.print("OK:Dummy Not Implemented");
                  }
                  else
                  {
                    client.print("Fail:Dummy");
                  }
                } 
                break;
                case d_clearCMD:
                  {
                    int index = other;
                    softataDevice_Display = GetDisplayFromList(index);
                    Tristate ts = softataDevice_Display->Clear();
                    if(ts == _ok)
                    {
                      client.print("OK:Clear");
                    }
                    else if (ts==notImplemented)
                    {
                      client.print("OK:Clear Not Implemented");
                    }
                    else
                    {
                      client.print("Fail:Clear");
                    }
                  } 
                  break;
                case d_home:
                  {
                    int index = other;
                    softataDevice_Display = GetDisplayFromList(index);
                    Tristate ts = softataDevice_Display->Home();
                    if(ts == _ok)
                    {
                      client.print("OK:Home");
                    }
                    else if (ts==notImplemented)
                    {
                      client.print("OK:Home Not Implemented");
                    }
                    else
                    {
                      client.print("Fail:Home");
                    }
                  } 
                  break;
                case d_backlightCMD:
                  {
                    int index = other;
                    softataDevice_Display = GetDisplayFromList(index);
                    Tristate ts = softataDevice_Display->Backlight();
                    if(ts == _ok)
                    {
                      client.print("OK:Backlight");
                    }
                    else if (ts==notImplemented)
                    {
                      client.print("OK:Backlight Not Implemented");
                    }
                    else
                    {
                      client.print("Fail:Backlight");
                    }
                  }
                  break;
                case d_setCursorCMD:
                  {
                    int index = other;
                    softataDevice_Display = GetDisplayFromList(index);
                    if(otherData[0]<2)
                    {
                      client.print("Fail:SetCursor needs (x,y)");
                    }
                    else
                    {
                      byte x = otherData[1];
                      byte y = otherData[2];
                      Tristate ts = softataDevice_Display->SetCursor(x,y);
                      if(ts == _ok)
                      {
                        client.print("OK:SetCursor");
                      }
                      else if (ts==notImplemented)
                      {
                        client.print("OK:SetCursor Not Implemented");
                      }
                      else
                      {
                        client.print("Fail:SetCursor");
                      }
                    }
                  }
                  break;
                case d_writestringCMD:
                {
                  int index = other;
                  softataDevice_Display = GetDisplayFromList(index);
                  if(otherData[0]<0)
                  {
                    client.print("Fail:WriteString needs data");
                  }
                  else
                  {
                    String msgStr =String("");
                    int stringLen = 0;
                    if(otherData[0]>1)
                    {
                      stringLen = otherData[0];
                      char * msg = (char *) (otherData + 1);
                      msgStr = String(msg);
                    }
                    Tristate ts = softataDevice_Display->WriteString(msgStr);
                    if(ts == _ok)
                    {
                      client.print("OK:WriteString");
                    }
                    else if (ts==notImplemented)
                    {
                      client.print("OK:WriteString Not Implemented");
                    }
                    else if (ts == _nan)
                    {
                      // For display that expects a number as string eg. Pixel
                      client.print("OK:WriteString NAN");
                    }
                    else
                    {
                      client.print("Fail:WriteString");
                    }
                  }
                }
                break;
                case d_cursor_writestringCMD:
                {
                  int index = other;
                  softataDevice_Display = GetDisplayFromList(index);
                  if(otherData[0]<2)
                  {
                    Serial_print("Fail:SetCursor-WriteString needs (x,y)");
                    client.print("Fail:SetCursor-WriteString needs (x,y)");
                  }
                  else
                  {
                    byte x = otherData[1];
                    byte y = otherData[2];
                    String msgStr =String("");
                    int stringLen = 0;
                    if(otherData[0]>3)
                    {
                      stringLen = otherData[0]-2;
                      char * msg = (char *) (otherData + 3);
                      msgStr = String(msg);
                    }
                    // Nb: Use blank string if there is an issue.

                    // Check if can cursor and write as one command.
                    Tristate ts = softataDevice_Display->CursorWriteStringAvailable();
                    if( ts == _nok)
                    {
                      // Can set cursor then write string instead
                      ts = softataDevice_Display->SetCursor(x,y);
                      if(ts == _ok)
                      {
                        ts = softataDevice_Display->WriteString(msgStr);
                        if(ts == _ok )
                        {
                          client.print("OK:SetCursor-then-WriteString");
                        }
                        else if(ts==notImplemented )
                        {
                          client.print("OK:SetCursor-then-WriteString-at-WriteString Not Implemneted");
                        }
                        else
                        {
                          client.print("Fail:SetCursor-then-WriteString-at-WriteString");
                        }
                      }
                      else
                      {
                        client.print("Fail:SetCursor-then-WriteString-at-SetCursor");
                      }
                    }
                    else if (ts == _ok)
                    {
                      // Can directly write string at x,y
                      Tristate ts = softataDevice_Display->WriteString(x,y,msgStr);
                      if(ts == _ok)
                      {
                        client.print("OK:Cursor_WriteStringXY");
                      }
                      else if(ts == notImplemented)
                      {
                        client.print("OK:Cursor_WritestringXY Not Implemented 1");
                      }
                      else
                      {
                        client.print("Fail:Cursor_WritestringXY");
                      }
                    }
                    else if (ts == notImplemented)
                    {
                      // No cursor and or no writestring
                      client.print("OK:Cursor_WritestringXY Not Implemented 2");
                    }
                  }              
                }
                break;              
                case d_miscCMD:
                  {
                    if(otherData[0]<1)
                    {
                      client.print("Fail:Display.Misc needs a command)");
                    }
                    else
                    {
                      int index=other;
                      Serial_println("Display->Misc cmd");
                      softataDevice_Display = GetDisplayFromList(index);
                      if(softataDevice_Display==NULL)
                      {
                        client.print("Fail:Display.Misc() NULL");
                      }
                      else
                      {
                        byte miscCMD = otherData[1];
                        Serial_print("miscCMD:");
                        Serial_println(miscCMD);
                        byte * miscData = NULL;
                        byte miscDataLength = otherData[0]-1;
                        if (miscDataLength>0)
                        {
                          miscData = otherData +2;
                        }
                        Serial_print("miscDataLength: ");
                        Serial_println(miscDataLength);
                        Tristate ts = softataDevice_Display->Misc(miscCMD,miscData,miscDataLength);
                        if(ts == _ok)
                        {
                          Serial_println("Misc OK");
                          client.print("OK:Display.Misc");
                        }
                        else if(ts==notImplemented)
                        {
                          Serial_println("Misc OK Not implemented");
                          client.print("OK:Display.Misc Not Implemented");
                        }
                        else
                        {
                          Serial_println("Misc Fail");
                          client.print("Fail:Display.Misc()");
                        }
                      }
                    }
                  }  
                  break;
                case D_dispose:
                  {
                    int index=other;
                    softataDevice_Display = GetDisplayFromList(index);
                    RemoveDisplayFromList(index);
                  } 
                  break;                             
                case D_getDevicesCMD:
                  {
                    Serial_println("Get Displays.");
                    String msg = String("OK:");
                    msg.concat(SoftataDevice_Display::GetListofDevices());
                    Serial_println(msg);
                    client.print(msg);
                  }
                  break;
                  default:
                    Serial_println("Display Unknown cmd");
                    client.print("OK: Unknown cmd");
                    break;
                
              }
            }
            break;
          case SOFTATADEVICE_ACTUATOR_CMD: //Grove Actuators
            {
              Tristate ts;
              SoftataActuator _actuator = (SoftataActuator)other;
              switch (param)
              {
                case A_getPinsCMD: //getPins
                  {
                    String pins ="OK:";
                    pins.concat(SoftataDevice_Actuator::GetPinout (_actuator));
                    client.print(pins);
                  }
                  break;
                  case A_getCmdsCMD: //get Generic Actuator Cmds
                  {
                    String cmds ="OK:";
                    cmds.concat(SoftataDevice_Actuator::GetListofCmds());
                    Serial.println(cmds);
                    client.print(cmds);
                  }
                  break;
                case a__getValueRangeCMD:
                  {
                    String valueRange ="OK:";
                    valueRange.concat(SoftataDevice_Actuator::GetRange(_actuator));
                    client.print(valueRange);
                  }
                  break;
                case A_setupDefaultCMD: //Setupdefault
                case A_setupGeneralCMD: //Setup(params)
                  {
                    Serial.println("Setup act");
                    SoftataDevice_Actuator * softatadevice_Actuator;
                    if(param==A_setupDefaultCMD)
                    {
                      softatadevice_Actuator = SoftataDevice_Actuator::_Setup(_actuator);
                      if(softatadevice_Actuator != NULL)
                      {
                        Serial_println("Default Actuator Setup");
                        int index = AddActuatorToList(softatadevice_Actuator);
                        Serial_print("Actuator Index: ");
                        Serial.println(index);
                        String msgSettingsA1 = "OK";
                        msgSettingsA1.concat(':');
                        msgSettingsA1.concat(index);
                        client.print(msgSettingsA1);
                      }
                      else
                        client.print("Fail:Actuator.Setup");
                      }
                    else
                    {
                      byte * asettings; 
                      byte numBytes = 1;                 
                      if(otherData==NULL)
                      {
                        asettings = new byte[1];//(byte *)malloc(1);
                        numBytes = 1;
                      }
                      else
                      {
                        if(otherData[0]>0)
                        {
                          Serial.println("Set num_bits to:");
                          asettings = new byte[2]; //(byte *)malloc(2);
                          byte num_bits = otherData[1]; 
                          Serial.println(num_bits);
                          asettings[1] = num_bits;
                          numBytes = 2;
                        }
                      }
                      Serial_print("Non-Default Actuator Setup Pin: ");
                      Serial_print(pin);
                      asettings[0] = pin;
                      softatadevice_Actuator = SoftataDevice_Actuator::_Setup(_actuator,asettings,numBytes);
                      delete[]  asettings; // free(asettings);
                      if(softatadevice_Actuator != NULL)
                      {
                        int index = AddActuatorToList(softatadevice_Actuator);
                        Serial_print("Actuator Index: ");
                        Serial_println(index);
                        String msgSettingsA2 = "OK";
                        msgSettingsA2.concat(':');
                        msgSettingsA2.concat(index);
                        client.print(msgSettingsA2);
                      }
                      else
                        client.print("Fail:Actuator.Setup");
                    }
                  }
                  break;  
                case a_GetnumbitsCMD:
                {
                  int index = other;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  String msg = String("OK:");
                  msg.concat(softatadevice_Actuator->GetNumBits());
                  Serial.println(msg);
                  client.print(msg);
                }  
                break; 
                case a_GetInstanceValueRangeCMD:
                {
                  int index = other;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  String msg = String("OK:0...");
                  msg.concat(softatadevice_Actuator->GetInstanceValueRange());
                  client.print(msg);
                }  
                break;  
                case a_GetActuatorCapabCMD:
                {
                  int index = other;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  String actuatorcapabilitiesStr ="OK:";
                  actuatorcapabilitiesStr.concat(softatadevice_Actuator->GetActuatorCapabilities());
                  client.print(actuatorcapabilitiesStr);               
                } 
                break;                               
                case a_writeDoubleValueCMD:
                {
                  int index = other;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteDoubleValue needs a value");
                  }
                  else
                  {
                    double value = (double)otherData[1];
                    ts = softatadevice_Actuator->Write(value,index);
                    if(ts == _ok)
                    {
                      client.print("OK:writeDoubleValue");
                    }
                    else if (ts==notImplemented)
                    {
                      client.print("OK:writeDoubleValue Not Implemented");
                    }
                    else if (ts==invalidParams)
                    {
                      client.print("OK:writeDoubleValue Invalid parameter/s");
                    }
                    else
                    {
                      client.print("Fail:writeDoubleValue");
                    }
                  }
                }
                break;
              case a_writeByteValueCMD:
                {
                  // Nb: Can submit byte as pin
                  int index = other;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  byte value  = pin;
                  if(otherData == NULL)
                  {
                    // Use pin
                  }
                  else if(otherData[0]<1)
                  {
                    // Use pin
                  }
                  else
                  {
                    byte value = otherData[1];
                  }
                  ts = softatadevice_Actuator->Write(value,index);

                  if(ts == _ok)
                  {
                    client.print("OK:writeByteValue");
                  }
                  else if (ts==notImplemented)
                  {
                    client.print("OK:writeByteValue Not Implemented");
                  }
                  else if (ts==invalidParams)
                  {
                    client.print("OK:writeByteValue Invalid parameter/s");
                  }
                  else
                  {
                    client.print("Fail:writeByteValue");
                  }
                  
                }
                break; 
               case a_writeWordValueCMD:
                {
                  int index = other;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  // Can submit as one byte as pin
                  int value = pin;
                  if(otherData == NULL)
                  {
                    //client.print("Fail:Actuator-WriteWordValue needs a value. otherData was NULL");
                  }
                  else if(otherData[0]<1)
                  {
                    //client.print("Fail:Actuator-WriteWordValue needs a value. otherData was Empty");
                  }
                  if(otherData[0]>0)
                    value = otherData[1];
                  if(otherData[0]>1)
                    value += otherData[2]*256;
                  if(otherData[0]>2)
                    value += otherData[3]*256*256;
                  if(otherData[0]>3)
                    value += otherData[4]*256*256*256;

                  Serial_print("Actuator: ");
                  Serial_print(value);
                  Serial_print('-');
                  Serial_println(index);

                  ts = softatadevice_Actuator->Write(value,index,2);
                  if(ts == _ok)
                  {
                    client.print("OK:writeWordValue");
                  }
                  else if (ts==notImplemented)
                  {;
                    client.print("OK:writeWordValue Not Implemented");
                  }
                  else if (ts==invalidParams)
                  {
                    client.print("OK:writeWordValue Invalid parameter/s");
                  }
                  else
                  {
                    client.print("Fail:writeWordValue");
                  }
                }
                break;                
              case a_SetBitStateCMD:  
              case a_SetBitCMD:
              case a_ClearBitCMD:
              case a_ToggleBitCMD:
                {
                  int index = other;
                  bool bitState = false;
                  byte stateValue =0;
                  SoftataDevice_Actuator * softatadevice_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-Set/Clear/Toggle needs a bit no.");
                  }
                  else
                  {
                    byte bit = otherData[1];
                    Serial_print("Actuator Param: ");
                    Serial_print(param);
                    Serial_print(" Bit: ");
                    Serial_print(bit);
                    Serial_print(" Index: ");
                    Serial_println(index);
                    switch(param)
                    {
                      case a_SetBitStateCMD:
                        stateValue = otherData[2];
                        if (stateValue>0)
                            bitState = true;
                        Serial_print('-');
                        Serial_print(bitState);
                        ts = softatadevice_Actuator->SetBitState(bitState,bit);
                        if(ts == _ok)
                        {
                          client.print("OK:SetBitState");
                        }
                        else if (ts==notImplemented)
                        {
                          client.print("OK:SetBitState Not Implemented");
                        }
                        else if (ts==invalidParams)
                        {
                          client.print("OK:SetBitState Invalid parameter/s");
                        }
                        else
                        {
                          client.print("Fail:SetBitState");
                        }
                        break;
                      case a_SetBitCMD:
                        ts = softatadevice_Actuator->SetBit(bit);
                        if(ts == _ok)
                        {
                          client.print("OK:SetBit");
                        }
                        else if (ts==notImplemented)
                        {
                          client.print("OK:SetBit Not Implemented");
                        }
                        else if (ts==invalidParams)
                        {
                          client.print("OK:SetBit Invalid parameter/s");
                        }                       
                        else
                        {
                          client.print("Fail:SetBit");
                        }
                        break;                      
                      case a_ClearBitCMD:
                        ts = softatadevice_Actuator->ClearBit(bit);
                        if(ts == _ok)
                        {
                          client.print("OK:ClearBit");
                        }
                        else if (ts==notImplemented)
                        {
                          client.print("OK:ClearBit Not Implemented");
                        }
                        else if (ts==invalidParams)
                        {
                          client.print("OK:ClearBit Invalid parameter/s");
                        }
                        else
                        {
                          client.print("Fail:ClearBit");
                        }  
                        break;                   
                      case a_ToggleBitCMD:
                        ts = softatadevice_Actuator->ToggleBit(bit);
                        if(ts == _ok)
                        {
                          client.print("OK:ToggleBit");
                        }
                        else if (ts==notImplemented)
                        {
                          client.print("OK:ToggleBit Not Implemented");
                        }
                        else if (ts==invalidParams)
                        {
                          client.print("OK:ToggleBit Invalid parameter/s");
                        }
                        else
                        {
                          client.print("Fail:ToggleBit");
                        }  
                        break;
                    }
                    Serial_println();
                    break;
                  }
                }
                break;                                                                     
              case A_getDevicesCMD:
                {
                  String msg = String("OK:");
                  msg.concat(SoftataDevice_Actuator::GetListofDevices());
                  client.print(msg);
                }
                break;
              default:
                    Serial_println("Actuator Unknown cmd");
                    client.print("OK:Actuator Unknown cmd");
                    break;
              }
            }
            break;        
          case SOFTATADEVICE_DEVICEINPUT_CMD: //Grove Device Iputs
            {            
              Tristate ts;
              SoftataDeviceInput _input = (SoftataDeviceInput)other;
              Serial.print("Input Param: ");
              Serial.println(param);
              switch (param)
              {
                case I_getCmdsCMD: //get Generic Input Cmds
                  {
                    String cmds ="OK:";
                    cmds.concat(SoftataDevice_DeviceInput::GetListofCmds());
                    Serial.println(cmds);
                    client.print(cmds);
                  }
                  break;
                case I_getPinsCMD: //getPins
                  {
                    String pins ="OK:";
                    pins.concat(SoftataDevice_DeviceInput::GetPinout (_input));
                    client.print(pins);
                  }
                  break;
                case i__getValueRangeCMD:
                  {
                    String valueRange ="OK:";
                    valueRange.concat(SoftataDevice_DeviceInput::GetRange(_input));
                    client.print(valueRange);
                  }
                  break;
                case I_setupDefaultCMD: //Setupdefault
                case I_setupGeneralCMD: //Setup(params)
                {
                    Serial.println("Setup input");
                    SoftataDevice_DeviceInput * softatadevice_DeviceInput;
                    if(param==I_setupDefaultCMD)
                    {
                      softatadevice_DeviceInput = SoftataDevice_DeviceInput::_Setup(_input);
                      if(softatadevice_DeviceInput != NULL)
                      {
                        Serial_println("Default DeviceInput Setup");
                        int index = AddDeviceInputToList(softatadevice_DeviceInput);
                        Serial_print("Device Index: ");
                        Serial.println(index);
                        String msgSettingsI1 = "OK";
                        msgSettingsI1.concat(':');
                        msgSettingsI1.concat(index);
                        client.print(msgSettingsI1);
                      }
                      else
                        client.print("Fail:DeviceInput.Setup");
                      }
                    else
                    {
                      byte * isettings; 
                      byte numBytes = 1;                 
                      if(otherData==NULL)
                      {
                        isettings = new byte[1];//(byte *)malloc(1);
                        numBytes = 1;
                      }
                      else
                      {
                        if(otherData[0]>0)
                        {
                          Serial.println("Set num_bits to:");
                          isettings = new byte[2]; //(byte *)malloc(2);
                          byte num_bits = otherData[1]; 
                          Serial.println(num_bits);
                          isettings[1] = num_bits;
                          numBytes = 2;
                        }
                      }
                      Serial_print("Non-Default DeviceInput Setup Pin: ");
                      Serial_print(pin);
                      isettings[0] = pin;
                      softatadevice_DeviceInput = SoftataDevice_DeviceInput::_Setup(_input,isettings,numBytes);
                      delete[]  isettings; // free(asettings);
                      if(softatadevice_DeviceInput != NULL)
                      {
                        int index = AddDeviceInputToList(softatadevice_DeviceInput);
                        Serial_print("DeviceInput Index: ");
                        Serial_println(index);
                        String msgSettingsI2 = "OK";
                        msgSettingsI2.concat(':');
                        msgSettingsI2.concat(index);
                        client.print(msgSettingsI2);
                      }
                      else
                        client.print("Fail:InputDevice.Setup");
                    }
                  }
                  break;  
                case I_getDevicesCMD:
                  {
                    String msg = String("OK:");
                    msg.concat(SoftataDevice_DeviceInput::GetListofDevices());
                    client.print(msg);
                  }
                  break;
                  case i_PollBitCMD:
                  {
                    Serial.println("Polling");
                    int index = other;
                    SoftataDevice_DeviceInput * softatadevice_DeviceInput = GetDeviceInputFromList(index);
                    String msg = String("OK:");
                    if(
                      ((softatadevice_DeviceInput->GetInputCapabilities() &  i_none ) !=  i_none )
                      &&
                      ((softatadevice_DeviceInput->GetInputCapabilities() &  i_bit ) !=  i_bit )
                    )
                      msg.concat("Not implemented");
                    else 
                      msg.concat(softatadevice_DeviceInput->PollBit());
                    client.print(msg);
                  }
                  break;
                /*
7 C(i_readByteValueCMD)
8 C(i_readWordValueCMD)

                */
              case i_GetnumbitsCMD:
                {
                  int index = other;
                  SoftataDevice_DeviceInput * softatadevice_DeviceInput = GetDeviceInputFromList(index);
                  String msg = String("OK:");
                  msg.concat(softatadevice_DeviceInput->GetNumBits());
                  client.print(msg);
                }  
                break; 
              case i_GetInstanceValueRangeCMD:
                {
                  int index = other;
                  SoftataDevice_DeviceInput * softatadevice_DeviceInput = GetDeviceInputFromList(index);
                  String msg = String("OK:0...");
                  msg.concat(softatadevice_DeviceInput->GetInstanceValueRange());
                  client.print(msg);
                }  
                break;  
                case i_GetInputCapabCMD:
                {
                  int index = other;
                  SoftataDevice_DeviceInput * softatadevice_DeviceInput = GetDeviceInputFromList(index);
                  String msg ="OK:";
                  msg.concat(softatadevice_DeviceInput->GetInputCapabilities());
                  client.print(msg);               
                }                                
                case i_readWordValueCMD:
                  {
                    int index = other;
                    SoftataDevice_DeviceInput * softatadevice_DeviceInput = GetDeviceInputFromList(index);

                      word val = softatadevice_DeviceInput->readWord();
                      Serial.print("val:");
                      Serial.println(val);
                      String msg = "OK:";
                      if(val == 0xffff)
                      {
                          if((softatadevice_DeviceInput->GetInputCapabilities() &  i_readword ) !=  i_readword )
                            msg.concat("Not implemented");
                          else
                            msg.concat(val);
                      }
                      else 
                        msg.concat(val);
                      client.print(msg);
                  }
                break;
                case i_readByteValueCMD:
                  {
                    int index = other;
                    SoftataDevice_DeviceInput * softatadevice_DeviceInput = GetDeviceInputFromList(index);

                      byte val = softatadevice_DeviceInput->readByte();
                      String msg = "OK:";
                      if(val == 0xff)
                      {
                          if((softatadevice_DeviceInput->GetInputCapabilities() &  i_readbyte ) !=  i_readbyte )
                            msg.concat("Not implemented");
                          else
                            msg.concat(val);
                      }
                      else
                        msg.concat(val);
                      Serial.println(msg);
                      client.print(msg);
                  }
                  break;
                  
                  
                  default:
                      Serial_println("Input Unknown cmd");
                      client.print("OK:Input Unknown cmd");
                      break;
                }
            }
            break;
          default:
            Serial_println("Unknown cmd");
            client.print("OK:Unknown cmd");
            break;
        }
        break;
 
    }
    delay(100);
  }

  if (client.connected())
  {
      client.flush();
  }
  client.stop();
  Serial_println("Client disconnected from Service OK.");

  ///////////////////
}


// Core2 code moved from here to:
#include "SoftataCore2.h"
/*
#include <HTTPClient.h>
void doCurl()
{
      HTTPClient client;

    Serial.print("[HTTPS] begin...\n");
    if (http.begin(*client, "https://data.mongodb-api.com/app/xxxxx/endpoint/data/beta/action/find")) {  // HTTPS
      
      http.addHeader("Content-Type", "application/json"); 
      http.addHeader("api-key", "wf19sEt..........fOBWhP8Q");
      
      String payload = "{\r\n\"collection\":\"myCollection\",\r\n\"database\":\"myDB\",\r\n\"dataSource\":\"Cluster0\",\r\n\"filter\": {\"FTX\": \"0001\"}\r\n}"; //Instead of TEXT as Payload, can be JSON as Paylaod
      
      Serial.print("[HTTPS] GET...\n");
      // start connection and send HTTP header
      int httpCode = http.POST(payload);

      // httpCode will be negative on error
      if (httpCode > 0) {
        // HTTP header has been send and Server response header has been handled
        Serial.printf("[HTTPS] GET... code: %d\n", httpCode);

        // file found at server
        if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_MOVED_PERMANENTLY) {
          String payload = https.getString();
          Serial.println(payload);
        }
      } else {
        Serial.printf("[HTTPS] GET... failed, error: %s\n", https.errorToString(httpCode).c_str());
      }

      https.end();
    } else {
      Serial.printf("[HTTPS] Unable to connect\n");
    }
  }
}
*/
