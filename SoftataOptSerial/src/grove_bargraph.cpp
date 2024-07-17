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
  // For simplicity for now, assume the app does send a valid string
  int num = msg.toInt();
  ic595->Write(num);
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
      return true;
      break;
    case flow2:
      //Just set even  segment for now.
      ic595->Write((byte)170);
      Serial_println("flow2()");
      return true;
      break;
    default:
      return false;
      break;
  }
}