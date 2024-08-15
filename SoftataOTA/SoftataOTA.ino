// RPi Pico BSP placed in the public domain by Earle F. Philhower, III, 2022

#include "softata.h"
#include "rpiboards.h"

#include <WiFi.h>
#include <LEAmDNS.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>



#include "SoftataOTA.h"
#include "Connect2Wifi.h"
#include "menu.h"


#include "rpiwatchdog.h"
#include "src/grove.h"
#include "src/grove_sensor.h"
#include "src/grove_environsensors.h"
#include "src/grove_actuator.h"
#include "src/grove_displays.h"

#include <float.h>

#include "devicesLists.h"

String Hostname = "picow2";

bool useSerialDebug = false;

struct CallbackInfo  BME280CBInfo;
// Used when first connected to change inbuilt LED blink rate.
bool hasConnected = false;


// Use one or other in testing
// Only used when Serial1/2 Setup is called from client.
// Comment out both normally.
//#define SERIAL1LOOPBACK
//#define SERIAL2LOOPBACK

//Grove_Sensor *grove_Sensor;

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

  ConnectMode connectMode = WIFI_CONNECT_MODE;
  if(useSerialDebug)
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

  bool Connect2WiFiConnected  = FlashStorage::WiFiConnectwithOptions(connectMode, useSerialDebug);
Connect2WiFiConnected= false;
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
  Hostname = FlashStorage::GetDeviceHostname();
  

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

//Grove_Sensor *grove_Sensor;


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
    Serial.println("Use Serial Debug? [Y](Default) [N]");
    Serial_println();
    // Longer wait as first menu choice 
    useSerialDebug = GetMenuChoiceYN(true, 2*DEFAULT_MENU_TIMEOUT_SEC);
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

  InitSensorList();
  InitDisplayList();
  InitActuatorList();  
  
  bool first = false;
  // Just to be safe don't simulataneously setup server and client
  uint32_t val = initialSynch;
 
  Serial_print("Initial Core1-Core2 Synch value:");
  Serial_println(val);
  rp2040.fifo.push(val);

  uint32_t sync = rp2040.fifo.pop();
  if(val==sync)
  {
    Serial_println("Core1-Core2 Setup Sync OK");
  }
  else
  {
    Serial_println("Core1-Core2 Setup Sync Fail");
  }
  #ifdef ENABLE_WATCHDOG
    watchdog_enable(WATCHDOG_SECS * 1000, false);
  #endif
}

void loop() {
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

   WiFiClient client = server.accept();//server.available();
  if (!client) {
    return;
  }
  Serial_println("WiFi-Server Up.");

  ////////////////////
  while (true) {
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
        String devicesCSV = Grove::GetListofDevices();
        Serial_println(devicesCSV);
        client.print(devicesCSV);
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
              Serial_print("Pin not Analog");
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
          case 0xB1:
            if (!IS_PIN_PWM(pin)) {
              Serial_print("Pin not PWM");
              client.print("FAIL");
              continue;
            }
            if (cmd == 0xB1) {
              Serial_print("PWM");
              analogWrite(pin, param);
              Serial_println("PWM:analogWrite()");
              client.print("OK");
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
          case 0xF0: //Grove Sensors
            { 
              // Cmd = 0xF0,pin=Index of cmd in list,param=SubCmd,Other=
              // GroveSensorCmds{
              // s_getpinsCMD, s_getPropertiesCMD, 
              // s_setupdefaultCMD, s_setupCMD, s_readallCMD, s_readCMD, s_getSensorsCMD=255 
              //};
              switch (param)
              {
                case s_getpinsCMD: 
                  {            
                    switch ((GroveSensor)other)
                    {
                      //#define G_SENSORS C(DHT11)C(BME280)C(UltrasonicRanger)
                      case DHT11:
                        {
                          client.print(Grove_DHT11::GetPins());
                        }
                        break;
                      case BME280:
                        {
                          client.print(Grove_BME280::GetPins());
                        }
                        break;
                      case UltrasonicRanger:
                        {
                          client.print(Grove_URangeSensor::GetPins());
                        }
                        break;
                      // Add more here
                      default:
                        String msgFail = "Fail:Not a sensor:";
                        msgFail += other;
                        client.print(msgFail);
                        break;
                    }
                  }
                  break;
                case s_getPropertiesCMD:
                  {
                    switch ((GroveSensor)other)
                    {
                      //#define SENSORS C(DHT11)C(SWITCH)C(SOUND)C(BME280)
                      case DHT11:
                        {
                          client.print(Grove_DHT11::GetListofProperties());
                        }
                        break;
                      case BME280:
                        {
                          client.print(Grove_BME280::GetListofProperties());
                        }
                        break;
                      case UltrasonicRanger:
                        {
                          client.print(Grove_URangeSensor::GetListofProperties());
                        }
                        break;                       
                      // Add more here
                      default:
                      client.print(Grove_URangeSensor::GetListofProperties());
                        String msgFail = "Fail:Not a sensor:";
                        msgFail += other;
                        client.print(msgFail);
                        break;
                    }

                  }
                  break;
                case s_setupdefaultCMD:
                case s_setupCMD:
                  {
                    Grove_Sensor * grove_Sensor;
                    GroveSensor groveSensor;
                    bool _done=false;
                    switch ((GroveSensor)other)
                    {
                      case DHT11:
                        {
                          grove_Sensor  = new Grove_DHT11();
                          _done = true;
                        }
                        break;
                      case BME280:
                        {
                          grove_Sensor  = new Grove_BME280();
                          _done = true;
                        }
                        break;
                      case UltrasonicRanger:
                        {
                          grove_Sensor  = new Grove_URangeSensor(DEFAULT_URANGE_PIN);
                          _done = true;
                        }
                        break;
                      // Add more here
                      default:
                        client.print("Fail:Not a sensor");
                        break;
                    }
                    if(_done)
                    {
                      if(param==s_setupdefaultCMD)
                      {
                        //Default setup
                        if(grove_Sensor->Setup())
                        {
                          int index = AddSensorToList(grove_Sensor);
                          String msgSettings1 = "OK";
                          msgSettings1.concat(':');
                          msgSettings1.concat(index);
                          client.print(msgSettings1);
                        }
                        else
                          client.print("Fail:Setup");
                        }
                      else
                      {
                        // Non-default setup
                        byte settings[1];
                        settings[0] = pin;
                        if(grove_Sensor->Setup(settings,1))
                        {
                          int index = AddSensorToList(grove_Sensor);
                          String msgSettings2 = "OK";
                          msgSettings2.concat(':');
                          msgSettings2.concat(index);
                          client.print(msgSettings2);
                        }
                        else
                          client.print("Fail:Setup");
                      }
                    }
                  }
                  break;
                case s_readallCMD:
                  {
                    int index = other;
                    Grove_Sensor * grove_Sensor = GetSensorFromList(index);
                    int numProps = grove_Sensor->num_properties;
                    double values[MAX_SENSOR_PROPERTIES];
                    if(grove_Sensor->ReadAll(values))
                    {                 
                      String msgGetAll = "OK:";
                      for (int i=0;i< numProps;i++)
                      {
                        msgGetAll.concat(values[i]);
                        if(i!= (numProps-1))
                          msgGetAll.concat(',');
                      }
                      client.print(msgGetAll);
                    }
                    else
                    {
                      client.print("Fail");
                    }
                  }
                  break;
                case s_readCMD:
                  {

                    Grove_Sensor * grove_Sensor = GetSensorFromList(other);
                    // A bit of reuse of real-estate here:
                    byte property = pin;
                    if(property>(grove_Sensor->num_properties-1))
                    {
                      client.print("Fail:Read Property no. > no. properties");
                    }
                    else
                    {
                      double value = grove_Sensor->Read(property);
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
                    Grove_Sensor * grove_Sensor = GetSensorFromList(index);
                    String json = grove_Sensor->GetTelemetry();
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
                      Grove_Sensor * grove_Sensor  = node->Sensor;
                      // Note: Each sensor instance has a private CallbackInfo property
                      CallbackInfo * info = grove_Sensor->GetCallbackInfo();
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
                      Grove_Sensor * grove_Sensor  = node->Sensor;
                      // Note: Each sensor instance has a private CallbackInfo property
                      CallbackInfo * info = grove_Sensor->GetCallbackInfo();

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
                case s_getSensorsCMD:
                  {
                    String msg = String("OK:");
                    msg.concat(Grove_Sensor::GetListof());
                    Serial_println(msg);
                    client.print(msg);
                  }
                  break;
              }
            }
            break;
          case 0xF1: //Grove Displays
            {
              //#define G_DISPLAYS C(OLED096)C(LCD1602)C(NEOPIXEL)
              // enum GroveDisplayCmds{
              // d_getpinsCMD, d_tbdCMD, d_setupDefaultCMD, d_setupCMD, 
              // d_clearCMD,d_backlightCND,d_setCursorCMD,homeCMD,d_miscCMD, d_getDisplaysCMD=255 
              // }
              GroveDisplay display = (GroveDisplay)other;
              switch (param)
              {
                case d_getpinsCMD: //getPins
                {
                  switch (display)
                  {
                    case OLED096:
                      client.print(Grove_OLED096::GetPins());
                      break;
                    case LCD1602:
                      client.print(Grove_LCD1602::GetPins());
                      break;
                    case NEOPIXEL:
                      client.print(Adafruit_NeoPixel8::GetPins());
                      break;
                    case BARGRAPH:
                      client.print(Custom_Bargraph::GetPins());
                      break;
                    default:
                      client.print("Fail:Not a display");
                      break;                 }
                }
                  break;
                case d_tbdCMD: //Get list of Msic commands for device
                {
                  Serial_println("Get Display.Miscs.");
                  String msg ="";
                  switch (display)
                  {
                    case OLED096:
                      msg = "OK:Misc:drawCircle,drawFrame,test";
                      break;
                    case LCD1602:
                      msg = "OK:Misc:home,autoscroll,noautoscroll,blink,noblink";
                      break;
                    case NEOPIXEL:
                      msg = "OK:Misc:setpixelcolor,setpixelcolorAll,setpixelcolorOdds,setpixelcolorEvens,setBrightness,setN";
                      break;
                    case BARGRAPH:
                      msg = "OK:Misc:flow,flow2";
                      break;
                    default:
                        msg = "Fail:Not a display";
                        break;
                  }
                  Serial_println(msg);
                  client.print(msg);              
                }
                  break;
                case d_setupDefaultCMD: //Setupdefault
                case d_setupCMD: //Setup(params)
                  {
                    Grove_Display * grove_Display;
                    //GroveDisplay groveDisplay;
                    bool _done=false;
                    switch ((GroveDisplay)other)
                    {
                      case OLED096:
                        {
                          grove_Display  = new Grove_OLED096();
                          _done = true;
                        }
                        break;
                      case LCD1602:
                        {
                          grove_Display  = new Grove_LCD1602();
                          _done = true;
                        }
                        break;
                      case NEOPIXEL:
                        {
                          grove_Display  = new Adafruit_NeoPixel8();
                          _done = true;
                        }
                        break;
                      case BARGRAPH:
                        {
                          grove_Display  = new Custom_Bargraph();
                          _done = true;
                        }
                        break;
                      // Add more here
                      default:
                        client.print("Fail:Not a display");
                        break;
                    }
                    if(_done)
                    {
                      if(param==d_setupDefaultCMD)
                      {
                        if(grove_Display->Setup())
                        {
                          int index = AddDisplayToList(grove_Display);
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

                        byte settings[maxNumDisplaySettings]; // Allow 4 settings for nw.
                        settings[0] = pin;
                        int numSettings=1;
                        Serial_print("Settings non default pin:");
                        Serial_println(pin);
                        if (otherData!= NULL)
                        {
                          for (int i=0; ((i< otherData[0]) &&(i<=maxNumDisplaySettings));i++)
                          {
                            settings[i+1] = otherData[i+1];
                            numSettings++;
                          }
                        }
                        if(grove_Display->Setup(settings,numSettings))
                        {
                          int index = AddDisplayToList(grove_Display);
                          String msgSettingsD2 = "OK";
                          msgSettingsD2.concat(':');
                          msgSettingsD2.concat(index);
                          client.print(msgSettingsD2);
                        }
                        else
                          client.print("Fail:Display.Setup");
                      }
                    }
                  }
                  break;
                case d_clearCMD:
                  {
                    int index = other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    if(grove_Display->Clear())
                    {
                      client.print("OK:Clear");
                    }
                    else
                    {
                      client.print("Fail:Clear");
                    }
                  } 
                  break;
                case d_backlightCMD:
                  {
                    int index = other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    if(grove_Display->Backlight())
                    {
                      client.print("OK:Backlight");
                    }
                    else
                    {
                      client.print("Fail;Backlight");
                    }
                  }
                  break;
                case d_setCursorCMD:
                  {
                    int index = other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    if(otherData[0]<2)
                    {
                      client.print("Fail:SetCursor needs (x,y)");
                    }
                    else
                    {
                      byte x = otherData[1];
                      byte y = otherData[2];
                      if(grove_Display->SetCursor(x,y))
                      {
                        client.print("OK:SetCursor");
                      }
                      else
                      {
                        client.print("Fail:SetCursor");
                      }
                    }
                  }
                  break;
                case d_writestrngCMD:
                {
                  int index = other;
                  Grove_Display * grove_Display = GetDisplayFromList(index);
                  if(otherData[0]<0)
                  {
                    client.print("Fail:WriteString needs data");
                  }
                  else
                  {
                    String msgStr =String("");
                    if(otherData[0]>1)
                    {
                      char * msg = (char *) (otherData + 1);
                      msgStr = String(msg);
                    }
                    Serial_print("Message:");
                    Serial_println(msgStr);
                    if(grove_Display->WriteString(msgStr))
                    {
                      client.print("OK:WriteString");
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
                  Grove_Display * grove_Display = GetDisplayFromList(index);
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
                    if(otherData[0]>2)
                    {
                      char * msg = (char *) (otherData + 3);
                      msgStr = String(msg);
                    }
                    // Nb: Use blank string if there is an issue.
                    Serial_print("Message:");
                    Serial_println(msgStr);
                    if(!grove_Display->CursorWriteStringAvailable())
                    {
                      if(grove_Display->SetCursor(x,y))
                      {
                        if(grove_Display->WriteString(msgStr))
                        {
                          client.print("OK:SetCursor-then-WriteString");
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
                    else 
                    {
                      if(grove_Display->WriteString(x,y,msgStr))
                      {
                        client.print("OK:Cursor_WriteString");
                      }
                      else
                      {
                        client.print("Fail:SetCursor-WriteString");
                      }
                    }
                  }              
                }
                break;              
                case d_miscCMD:
                  {
                    Serial_print("d_miscCMD");Serial_print("-");Serial_println(d_miscCMD);
                    if(otherData[0]<1)
                    {
                      client.print("Fail:Display.Misc needs a command)");
                    }
                    else
                    {
                      int index=other;
                      Serial_println("Display->Misc cmd");
                      Grove_Display * grove_Display = GetDisplayFromList(index);
                      if(grove_Display==NULL)
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
                        Serial_println("miscDataLength:");
                        Serial_println(miscDataLength);
                        if(grove_Display->Misc(miscCMD,miscData,miscDataLength))
                        {
                          Serial_println("Misc OK");
                          client.print("OK:");
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
                case d_dispose:
                  {
                    int index=other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    RemoveDisplayFromList(index);
                  } 
                  break;                             
                case d_getDisplaysCMD:
                  {
                    Serial_println("Get Displays.");
                    String msg = String("OK:");
                    msg.concat(Grove_Display::GetListof());
                    Serial_println(msg);
                    client.print(msg);
                  }
                  break;
                
              }
            }
            break;
          case 0xF2: //Grove Actuators
            {
              GroveActuator actuator = (GroveActuator)other;
              switch (param)
              {
                case a_getpinsCMD: //getPins
                {
                  switch (actuator)
                  {
                    case SERVO:
                      client.print(Grove_Servo::GetPins());
                      break;
                    case SHIFT595PARAOUT:
                      client.print(Shift595ParaOut::GetPins());
                      break;
                    default:
                      client.print("Fail:Not an Actuator");
                      break;                 }
                  }
                  break;
                case a_getValueRangeCMD:
                {
                  switch (actuator)
                  {
                    case SERVO:
                      client.print(Grove_Servo::GetValueRange());
                      break;
                    case SHIFT595PARAOUT:
                      client.print(Shift595ParaOut::GetValueRange());
                      break;
                    default:
                        client.print("Fail:Not an actuator");
                        break;
                  }
                }
                  break;
                case a_setupDefaultCMD: //Setupdefault
                case a_setupCMD: //Setup(params)
                  {
                    Grove_Actuator * grove_Actuator;
                    //GroveActuator groveActuator;
                    bool _done=false;
                    switch ((GroveActuator)other)
                    {
                      case SERVO:
                        {
                          grove_Actuator  = new Grove_Servo();
                          _done = true;
                        }
                        break;
                      case SHIFT595PARAOUT:
                        {
                          grove_Actuator  = new Shift595ParaOut();
                          _done = true;
                        }
                        break;
                      // Add more here
                      default:
                        client.print("Fail:Not an actuator");
                        break;
                    }
                    if(_done)
                    {
                      if(param==d_setupDefaultCMD)
                      {
                        if(grove_Actuator->Setup())
                        {
                          int index = AddActuatorToList(grove_Actuator);
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
                        byte settings[1];
                        settings[0] = pin;
                        if(grove_Actuator->Setup(settings,1))
                        {
                          int index = AddActuatorToList(grove_Actuator);
                          String msgSettingsA2 = "OK";
                          msgSettingsA2.concat(':');
                          msgSettingsA2.concat(index);
                          client.print(msgSettingsA2);
                        }
                        else
                          client.print("Fail:Actuator.Setup");
                      }
                    }
                  }
                  break;                                       
                case a_writeDoubleValueCMD:
                {
                  int index = other;
                  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteDoubleValue needs (a value");
                  }
                  else
                  {
                    double value = (double)otherData[1];
                    if(grove_Actuator->Write(value,index))
                      client.print("OK:Actuator-WriteDoubleValue");
                    else
                      client.print("Fail:Actuator-WriteDoubleValue");
                  }
                }
                break;
              case a_writeByteValueCMD:
                {
                  int index = other;
                  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteIntValue needs (a value");
                  }
                  else
                  {
                    byte value = otherData[1];
                    Serial_print("Actuator: ");
                    Serial_print(value);
                    Serial_print('-');
                    Serial_println(index);
                    if(grove_Actuator->Write(value,index))
                      client.print("OK:Actuator-WriteByte");
                    else
                      client.print("Fail:Actuator-WriteByte");
                  }
                }
                break; 
               case a_writeWordValueCMD:
                {
                  int index = other;
                  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteIntValue needs (a value");
                  }
                  else
                  {
                    int value = otherData[1]*256+otherData[2];
                    Serial_print("Actuator: ");
                    Serial_print(value);
                    Serial_print('-');
                    Serial_println(index);

                    if(grove_Actuator->Write(value,index,2))
                      client.print("OK:Actuator-WriteWordValue");
                    else
                      client.print("Fail:Actuator-WriteWordValue");
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
                  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteIntValue needs (a value");
                  }
                  else
                  {
                    byte bit = otherData[1];
                    Serial_print("Actuator: ");
                    Serial_print(bit);
                    Serial_print('-');
                    Serial_print(index);
                    switch(cmd)
                    {
                      case a_SetBitStateCMD:
                        stateValue = otherData[2];
                        if (stateValue>0)
                            bitState = true;
                        Serial_print('-');
                        Serial_print(bitState);
                        if(grove_Actuator->SetBitState(bitState,bit))
                          client.print("OK:Actuator-SetBitState");
                        else
                          client.print("Fail:Actuator-SetBitState");
                        break;
                      case a_SetBitCMD:
                        if(grove_Actuator->SetBit(bit))
                          client.print("OK:Actuator-SetBit");
                        else
                          client.print("Fail:Actuator-SetBit");
                        break;                      
                      case a_ClearBitCMD:
                          if(grove_Actuator->ClearBit(bit))
                          client.print("OK:Actuator-ClearBit");
                        else
                          client.print("Fail:Actuator-ClearBit");
                        break;                     
                      case a_ToggleBitCMD:
                        if(grove_Actuator->ToggleBit(bit))
                          client.print("OK:Actuator-ToggleBit");
                        else
                          client.print("Fail:Actuator-ToggleBit");
                        break;
                    }
                    Serial_println();
                    break;
                  }
                }
                break;                                                                     
              case a_getActuatorsCMD:
                {
                  String msg = String("OK:");
                  msg.concat(Grove_Actuator::GetListof());
                  client.print(msg);
                }
                break;
              }
            }
            break;        
 
          default:
            Serial_println("Unknown cmd");
            client.print("Unknown cmd");
            break;
        }
        break;
    }
    delay(500);
  }


  client.printf("Done from Pico-W\r\n");
  client.flush();

  ///////////////////
}


// Core2 code moved from here to:
#include "SoftataCore2.h"
