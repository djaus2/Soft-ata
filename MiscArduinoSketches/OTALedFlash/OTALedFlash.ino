#include <WiFi.h>
#include <LEAmDNS.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include "OTALedFlash.h"

const char* ssid = STASSID;
const char* password = STAPSK;
const char* host = "OTA-LEDS";

void flash(int num, int period)
{
  for (int i =0;i<num;i++)
  {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(period/2);
    digitalWrite(LED_BUILTIN, LOW);
    delay(period/2);
  }
}


void setup() {
  // initialize serial:
  Serial.begin(115200);
  while(!Serial);

  Serial.println("Booting");

  //Get SSID
  Serial.println("Enter SSID:");
  while (Serial.available() == 0) {delay(100);}
  String _ssid = Serial.readString();
  _ssid.trim();
  if(_ssid.length() != 0)
    ssid=_ssid.c_str();

  //Get Password
  Serial.println("Enter Password:");
  while (Serial.available() == 0) {delay(100);}
  String _password = Serial.readString();
  _password.trim();
  if(_password.length()!=0)
    password = _password.c_str();

    /* Use BuiltIn LED for flashes */
  pinMode(LED_BUILTIN, OUTPUT);

  // Booting: 5 short pulses
  flash(BOOTING_NUMFLASHES, BOOTING_PERIOD);

  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);

  while (WiFi.waitForConnectResult() != WL_CONNECTED) {
    delay(100);
  }

  // Got WiFi: 3 long pulses
  flash(WIFI_STARTED_NUMFLASHES, WIFI_STARTED_PERIOD);
  
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
    flash(OTA_ON_START_NUMFLASHES, OTA_ON_START_PERIOD);
    // Cause an error by making this too long:
    // flash(10, LONGPULSE);
  });

  ArduinoOTA.onEnd([]() {  // do a fancy thing with our board led at end
    // Done OTA:4 long pulses
    flash(OTA_ON_END_NUMFLASHES, OTA_ON_END_PERIOD);
  });

  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    // Error: 6 L-O-N-G pulses
    flash(OTA_ON_ERROR_NUMFLASHES, OTA_ON_ERROR__PERIOD);
    if (error == OTA_AUTH_ERROR) {
      Serial.println("Auth Failed");
      flash(2, SHORTPULSE);
    } else if (error == OTA_BEGIN_ERROR) {
      Serial.println("Begin Failed");
      flash(4, SHORTPULSE);
    } else if (error == OTA_CONNECT_ERROR) {
      Serial.println("Connect Failed");
      flash(6, SHORTPULSE);
    } else if (error == OTA_RECEIVE_ERROR) {
      Serial.println("Receive Failed");
      flash(8, SHORTPULSE);
    } else if (error == OTA_END_ERROR) {
      Serial.println("End Failed");
      flash(10, SHORTPULSE);
    }

    rp2040.restart();
  });
  /*******************************************************************/

  /* Setup the OTA server */
  ArduinoOTA.begin();
  Serial.println("Ready");
  // Ready: 8 ultra short pulses
  flash(READY_NUMFLASHES, READY_PERIOD);
}

void loop() {
  ArduinoOTA.handle();
}
