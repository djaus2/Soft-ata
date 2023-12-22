#include "testDHT11.h"
#include "grove_dht11.h"
#include <arduino.h>
Grove_DHT11 groveDHT11;

void TestDHT11_setup() {
  Serial.begin();
  groveDHT11.Setup();
  while(!Serial);
  groveDHT11.GetPins();
}

void TestDHT11_loop() {
  double values[2];
  bool result = groveDHT11.ReadAll(values);
  if(result)
  {
    Serial.print(values[0]);
    Serial.print(',');
    Serial.println(values[1]);
  }
  delay(500);
}

