#include <Arduino.h>
#include "grove_actuator.h"

Grove_Relay::Grove_Relay()
{
   
}

Grove_Relay::Grove_Relay(int pin)
{
    byte _relayPin = pin;
    Setup(&_relayPin,1);
}


bool Grove_Relay::Setup()
{
  byte _relayPin = 16;
  Setup(&_relayPin,1);
  return true;
}

bool Grove_Relay::Setup(byte * settings, byte numSettings)
{

  if(numSettings>0)
  {
    relayPin = settings[0];
  }
   else
   {
        Serial_print("Setting up Relay FAILED: No settings (pin no.)");
        return false;
   }
    Serial_print("Setting up relay on pin: ");
    Serial.println(relayPin);
    pinMode(relayPin, OUTPUT);
    digitalWrite(relayPin, false);
    return true;
}

// Index for if there are an array of actuators here.
// Instance of is collected elsewhere.
// No longer need. 2Do Remove

bool Grove_Relay::Write(double value, int index)
{
    return true;
}
bool Grove_Relay::Write(int value, int index, int numBytes )
{
    return true;
}


bool Grove_Relay::SetBitState(bool state,int bitNo)
{
    Serial.println("Setting relay state");
    digitalWrite(relayPin, state);
    return true;
}

bool Grove_Relay::SetBit(int bitNo )
{
    Serial.println("Setting relay");
    digitalWrite(relayPin, true);
    return true;
}

bool Grove_Relay::ClearBit(int bitNo)
{
    Serial.println("Clearing relay");
    digitalWrite(relayPin, false);
    return true;
}

bool Grove_Relay::ToggleBit(int bitNo)
{
    Serial.println("Toggling relay");
    bool state = digitalRead(relayPin);
    digitalWrite(relayPin, !state);
    return true;
}