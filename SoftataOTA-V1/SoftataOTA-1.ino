// RPi Pico BSP placed in the public domain by Earle F. Philhower, III, 2022
#include "softata.h"
#include "rpiboards.h"

#include <WiFi.h>
#include <LEAmDNS.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include "SoftataOTA.h"

#include "rpiwatchdog.h"
#include "src/grove.h"
#include "src/grove_sensor.h"
#include "src/grove_environsensors.h"
#include "src/grove_actuator.h"
#include "src/grove_displays.h"

#include <float.h>
#include "devicesLists.h"



const char* ssid = STASSID;
const char* password = STAPSK;

#define BUFFSIZE 128
byte Buffer[BUFFSIZE];

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
  Serial_begin(115200);
  OTAing =false;
  whileNotSerial()
  { 
    delay(100);
  }

  Serial_println("Booting");
  
  /* Use BuuiltIn LED for flashes */
  pinMode(LED_BUILTIN, OUTPUT);

  // Booting: 5 short pulses
  flash(0, BOOTING_NUMFLASHES, BOOTING_PERIOD);


  WiFi.mode(WIFI_STA);
  WiFi.setHostname("PicoW2");
  WiFi.begin(ssid, password);
  Serial_println("WIFI");
  while (WiFi.status() != WL_CONNECTED) {
    Serial_print(".");
    delay(100);
  }
  Serial_println("Connnected.");



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
;
  ArduinoOTA.onEnd([]() {  // do a fancy thing with our board led at end
    // Done OTA:4 long pulses
    Serial_println("OTA OnEnd");
    flash(3, OTA_ON_END_NUMFLASHES, OTA_ON_END_PERIOD);
  });

  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
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
bool hasConnected = false;

// Use one or other in testing
// Only used when Serial1/2 Setup is called from client.
// Comment out both normally.
//#define SERIAL1LOOPBACK
//#define SERIAL2LOOPBACK

//Grove_Sensor *grove_Sensor;

int port = PORT;
WiFiServer server(port);


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
  ArduinoOTAsetup();
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
  #ifdef USE_WATCHDOG
  watchdog_enable(WATCHDOG_SECS * 1000, false);
  #endif
}

void loop() {
  ArduinoOTAloop();
  #ifdef USE_WATCHDOG
  watchdog_update();
  #endif
  if(OTAing)
      return;
 
  Serial_print("-");
  static int i;
  int count = 0;
  byte msg[maxRecvdMsgBytes];
                              
  delay(500);

   WiFiClient client = server.accept();//server.available();
  if (!client) {
    return;
  }
  Serial_println("WiFi-Server Up.");
}


// Core2 code moved from here to:
#include "SoftataCore2.h"
