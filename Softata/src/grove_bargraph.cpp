#include <Arduino.h>
#include "grove_displays.h"

int dataPin = 16;   // Pin connected to DS of 74HC595(Pin14)  
int latchPin = 20;  // Pin connected to ST_CP of 74HC595(Pin12)
int clockPin = 21;  // Pin connected to SH_CP of 74HC595(Pin11)          

void writeTo595(BitOrder order, byte _data ) {
  // Output low level to latchPin
  digitalWrite(latchPin, LOW);
  // Send serial data to 74HC595
  shiftOut(dataPin, clockPin, order, _data);
  // Output high level to latchPin, and 74HC595 will update the data to the parallel output port.
  digitalWrite(latchPin, HIGH);
}

// Default Setup
bool Custom_Bargraph::Setup()
{
  byte settings[3];
  settings[0] = 16;
  settings[1] = 20;
  settings[2] = 21;
  return Setup(settings,3);
}

bool Custom_Bargraph::Setup(byte * settings, byte numSettings)
{
 
  if (numSettings >2)
  {
    dataPin = (int)settings[0];
    latchPin = (int)settings[1];
    clockPin = (int)settings[2];
  };
  pinMode(latchPin, OUTPUT);
  pinMode(clockPin, OUTPUT);
  pinMode(dataPin, OUTPUT);
  Serial.begin();
  return true;
}

bool Custom_Bargraph::Clear()
{
  writeTo595(LSBFIRST, 0);
  return true;
}

bool Custom_Bargraph::Backlight()
{
  return true;
}

bool Custom_Bargraph::Home()
{
    return true;
}


bool Custom_Bargraph::SetCursor(byte x, byte y)
{
  return true;
}

bool Custom_Bargraph::WriteString(String msg)
{
  // For simplicity for nwo, assume the Console app does send a valid string
  int num = msg.toInt();
  writeTo595(LSBFIRST, num);
  return true;
}

bool Custom_Bargraph::CursorWriteStringAvailable()
{
	return true;
}

bool Custom_Bargraph::WriteString(byte x, byte y, String msg)
{
  return true;
}

bool Custom_Bargraph::Misc(byte cmd, byte * data, byte length)
{
  BARGRAPHMiscCmds Cmd = (BARGRAPHMiscCmds)cmd;
  switch(Cmd)
  {
    case flow:
      //Just set every odd segment for now.
      writeTo595(LSBFIRST, (byte)85);
      Serial.println("flow()");
      return true;
      break;
    case flow2:
      //Just set even  segment for now.
      writeTo595(LSBFIRST, (byte)170);
      Serial.println("flow2()");
      return true;
      break;
    default:
      return false;
      break;
  }
}