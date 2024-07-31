#include <WiFi.h>
int status = WL_IDLE_STATUS;     // the Wifi radio's status
void setup()
{
  // initialize serial:
  Serial.begin(115200);
  while(!Serial);
  //Get SSID
  Serial.println("Enter SSID:");
  while (Serial.available() == 0) {}
  String ssid = Serial.readString();
  ssid.trim();
  //Get Password
  Serial.println("Enter Password:");
  while (Serial.available() == 0) {}
  String passwd = Serial.readString();
  passwd.trim();
  //WiFi Connect   
  WiFi.begin(ssid.c_str(),passwd.c_str());
  while ( WiFi.status() != WL_CONNECTED) {
    Serial.print('.');
    delay(250);
  }
  //print the local IP address
  IPAddress ip = WiFi.localIP();
  Serial.println(ip);
}
void loop () {}