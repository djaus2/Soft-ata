#include <Arduino.h>
#include "grove_displays.h"

// Custom_Bargraph


// Default Setup
bool Custom_Bargraph::Setup()
{
  ic595 = new IC_74HC595_ShiftRegister();
  return true;
}

bool Custom_Bargraph::Setup(byte * settings, byte numSettings)
{
  ic595 = new IC_74HC595_ShiftRegister(settings,numSettings);
  return true;
}

bool Custom_Bargraph::Clear()
{
  ic595->Write(0);
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
  int len = msg.length();
  if (len>10) len = 10;
  int numVal=0;
  String prefix = msg.substring(0,2);
  numStringType typ = _NONE;
  if(prefix == "0b" ){
    typ = _BIN;
  } else if( prefix == "0x"){
    typ = _HEX;
  }
  switch (typ){
    case _BIN:
      for (int i=0; i<len-2; i++){
        // shift over what's there
        numVal = numVal << 1;
        if(msg[len - i] == 1){
            numVal |= 1;
        }
      };
      break;
    case _HEX:
      numVal = (int) strtoull(msg.c_str(), 0, 16);
      numVal &= 0b00001111111111;
      break;
    default:
      Serial.println(msg);
      numVal = msg.toInt();
      numVal &= 0b00001111111111;
      break;
  }
  ic595->Write(numVal);
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
      ic595->Write((byte)85);
      Serial_println("flow()");
      break;
    case flow2:
      //Just set even  segment for now.
      ic595->Write((byte)170);
      Serial_println("flow2()");
      break;
    case setLed:
    case clrLed:
    case toggleLed:
      // Set or toggle a specific LED
      if (length < 1)
        return false;
      if (Cmd == setLed)
        ic595->SetBit(data[0]);
      else if (Cmd == clrLed)
        ic595->ClearBit(data[0]);
      else
        ic595->ToggleBit(data[0]);
      break;
    case setLevel:
      // Set the level of the bar
      if (length < 1)
        return false;
      ic595->SetLevel(data[0]);
      break;
    case exercise:
        // Turn on all LEDs
        ic595->Write(0x3ff);
        delay(1000);

        // Turn off all LEDs
        ic595->Write(0x0);
        break;
    default:
      return false;
      break;
  }
  return true;
}
